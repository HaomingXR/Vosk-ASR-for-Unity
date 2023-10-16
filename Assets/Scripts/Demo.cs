using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vosk.APIs;

/// <summary>
/// Usage Example
/// </summary>
public class Demo : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name of the Model")]
#if UNITY_EDITOR
    private const string ModelName = "vosk-model-small-cn-0.22";
#else
    private const string ModelName = "vosk-model-cn-0.22";
#endif

    [Tooltip("Should the recognizer start automatically")]
    [SerializeField]
    private bool AutoStart = true;

    [Tooltip("The Max number of alternatives that will be processed")]
    [SerializeField]
    private int MaxAlternatives = 1;

    [Tooltip("The index of Microphone to use")]
    [SerializeField]
    private int microphoneIndex = 0;

    [Tooltip("The phrases that will be detected. If left empty, all words will be detected.\nKeywords need to exist in the models dictionary, so some words like \"webview\" are better detected as two more common words \"web view\".")]
    [SerializeField]
    private List<string> KeyPhrases = new List<string>();

    [Header("UI")]
    [SerializeField]
    private Text ResultText;

    void OnEnable()
    {
        VoskASR.OnTranscriptionResult += OnTranscriptionResult;
    }

    void OnDisable()
    {
        VoskASR.OnTranscriptionResult -= OnTranscriptionResult;
    }

    void Start()
    {
        for (int i = 0; i < KeyPhrases.Count; i++)
            KeyPhrases[i] = StringFormatter.ChineseUtils.ToSimp(KeyPhrases[i].Trim());

        VoskASR.Init(this, ModelName, AutoStart, MaxAlternatives, microphoneIndex, KeyPhrases);
    }

    private void OnTranscriptionResult(string obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj);
#endif

        RecognitionResult resultJson = JsonConvert.DeserializeObject<RecognitionResult>(obj);
        ResultText.text += StringFormatter.RemoveSpaces(StringFormatter.ChineseUtils.ToTrad(resultJson.alternatives[0].text).Replace("[unk]", " "));
    }

    private struct RecognitionResult
    {
        public Tag[] alternatives;

        public class Tag
        {
            public float confidence;
            public string text;
        }
    }
}
