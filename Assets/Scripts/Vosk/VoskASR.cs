using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

namespace Vosk.APIs
{
	public static class VoskASR
	{
		// === User Configs ===

		private static MonoBehaviour instance;

		private static string ModelName;
		private static int MaxAlternatives;
		private static List<string> KeyPhrases;

		// === Internal Variables ===

		// Cached version of the Vosk Model.
		private static Model _model;

		// Cached version of the Vosk recognizer.
		private static VoskRecognizer _recognizer;

		// Conditional flag to see if a recognizer has already been created.
		private static bool _recognizerReady;

		// Called when the the state of the controller changes.
		public static Action<string> OnStatusUpdated;

		// Called after the user is done speaking and vosk processes the audio.
		public static Action<string> OnTranscriptionResult;

		// The absolute path to the decompressed model folder.
		private static string _modelPath;

		// A string that contains the keywords in Json Array format
		private static string _grammar = "";

		// Flag that is used to wait for the the script to start successfully.
		private static bool _isInitializing;

		// Flag that is used to check if Vosk was started.
		private static bool _didInit;

		// === Threading Logic ===

		// Flag to signal we are ending
		private static bool _running;

		// Thread safe queue of microphone data.
		private static readonly ConcurrentQueue<short[]> _threadedBufferQueue = new ConcurrentQueue<short[]>();

		// Thread safe queue of resuts
		private static readonly ConcurrentQueue<string> _threadedResultQueue = new ConcurrentQueue<string>();

		private static readonly ProfilerMarker voskRecognizerCreateMarker = new ProfilerMarker("VoskRecognizer.Create");
		private static readonly ProfilerMarker voskRecognizerReadMarker = new ProfilerMarker("VoskRecognizer.AcceptWaveform");

		/// <summary>
		/// Start Vosk Speech to Text
		/// </summary>
		/// <param name="caller">The MonoBehaviour to run the audio capturing on. The GameObject should never be destoryed.</param>
		/// <param name="modelName">The path to the model folder relative application streaming asset folder.</param>
		/// <param name="autoStart">"Should the microphone after vosk initializes.</param>
		/// <param name="maxAlternatives">The maximum number of alternative phrases detected.</param>
		/// <param name="microphoneIndex">The index of microphone to use.</param>
		/// <param name="keyPhrases">A list of keywords/phrases.</param>
		public static void Init(MonoBehaviour caller, string modelName, bool autoStart, int maxAlternatives, int microphoneIndex, List<string> keyPhrases)
		{
			VoiceProcessor.Init(caller, microphoneIndex);

			if (_isInitializing || _didInit)
			{
				Debug.LogWarning("Vosk has already been initialized!");
				return;
			}

			instance = caller;

			ModelName = modelName;
			MaxAlternatives = maxAlternatives;
			KeyPhrases = keyPhrases;

			instance.StartCoroutine(DoInitVoskS2T(autoStart));
		}

		/// <summary>
		/// Load model, load settings, start Vosk and optionally start the microphone
		/// </summary>
		private static IEnumerator DoInitVoskS2T(bool startMicrophone)
		{
			_isInitializing = true;
			yield return WaitForMicrophoneInput();

			_modelPath = Path.Combine(Application.streamingAssetsPath, ModelName);

#if UNITY_EDITOR
			Debug.Log(_modelPath);
#endif

			OnStatusUpdated?.Invoke("Loading Model from: " + _modelPath);

			_model = new Model(_modelPath);

			yield return null;

			OnStatusUpdated?.Invoke("Vosk Initialized");
			VoiceProcessor.OnFrameCaptured += VoiceProcessorOnOnFrameCaptured;
			VoiceProcessor.OnRecordingStop += VoiceProcessorOnOnRecordingStop;

			_isInitializing = false;
			_didInit = true;

			if (startMicrophone)
				StartRecording();
		}

		/// <summary>
		/// Can be called to start detection.
		/// </summary>
		public static void StartRecording()
		{
			if (!VoiceProcessor.IsRecording)
			{
				Debug.Log("[Vosk] Start Recording");

				try
				{
					_running = true;
					instance.StartCoroutine(Update());
					VoiceProcessor.StartRecording();
					Task.Run(ThreadedWork).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					_running = false;
					Debug.LogError($"Failed to start Recording: {e.Message}\n{e.StackTrace}");
				}
			}
			else
			{
				return;
			}
		}

		/// <summary>
		/// Can be called to stop detection.
		/// </summary>
		public static void StopRecording()
		{
			if (!VoiceProcessor.IsRecording)
			{
				return;
			}
			else
			{
				Debug.Log("[Vosk] Stop Recording");
				_running = false;
				VoiceProcessor.StopRecording();
			}
		}

		/// <summary>
		/// Translates the KeyPhraseses into a json array and appends the `[unk]` keyword at the end to tell vosk to filter other phrases.
		/// </summary>
		private static void UpdateGrammar()
		{
			if (KeyPhrases == null || KeyPhrases.Count == 0)
				_grammar = "";
			else
				_grammar = $"[\"{string.Join("\",\"", KeyPhrases)}\",\"[unk]\"]";
		}

		/// <summary>
		/// Wait until microphones are initialized
		/// </summary>
		private static IEnumerator WaitForMicrophoneInput()
		{
			while (Microphone.devices.Length <= 0)
				yield return null;
		}

		/// <summary>
		/// Calls the On Phrase Recognized event on the Unity Thread
		/// </summary>
		private static IEnumerator Update()
		{
			while (_running)
			{
				if (_threadedResultQueue.TryDequeue(out string voiceResult))
					OnTranscriptionResult?.Invoke(voiceResult);

				yield return null;
			}
		}

		/// <summary>
		/// Callback from the voice processor when new audio is detected
		/// </summary>
		private static void VoiceProcessorOnOnFrameCaptured(short[] samples)
		{
			_threadedBufferQueue.Enqueue(samples);
		}

		/// <summary>
		/// Callback from the voice processor when recording stops
		/// </summary>
		private static void VoiceProcessorOnOnRecordingStop()
		{
			Debug.Log("[Vosk] Recording Stopped");
		}

		/// <summary>
		/// Feeds the autio logic into the vosk recorgnizer
		/// </summary>
		private static async Task ThreadedWork()
		{
			voskRecognizerCreateMarker.Begin();

			if (!_recognizerReady)
			{
				UpdateGrammar();

				// Only detect defined keywords if they are specified.
				if (string.IsNullOrEmpty(_grammar))
					_recognizer = new VoskRecognizer(_model, 16000.0f);
				else
					_recognizer = new VoskRecognizer(_model, 16000.0f, _grammar);

				_recognizer.SetMaxAlternatives(MaxAlternatives);
				// _recognizer.SetWords(true);
				_recognizerReady = true;

				Debug.Log("[Vosk] Recognizer ready");
			}

			voskRecognizerCreateMarker.End();

			voskRecognizerReadMarker.Begin();

			while (_running)
			{
				if (_threadedBufferQueue.TryDequeue(out short[] voiceResult))
				{
					if (_recognizer.AcceptWaveform(voiceResult, voiceResult.Length))
					{
						var result = _recognizer.Result();
						_threadedResultQueue.Enqueue(result);
					}
				}
				else
				{
					// Wait for some data
					await Task.Delay(100);
				}
			}

			voskRecognizerReadMarker.End();
		}
	}
}
