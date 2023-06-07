using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vosk.APIs;

public class Demo : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name of the Model")]
    [SerializeField]
    private string ModelName = "vosk-model-small-cn-0.22";

    [Tooltip("Should the recognizer start automatically")]
    [SerializeField]
    private bool AutoStart = true;

    [Tooltip("The Max number of alternatives that will be processed.")]
    [SerializeField]
    private int MaxAlternatives = 2;

    [Tooltip("The index of Microphone to use.")]
    [SerializeField]
    private int microphoneIndex = 0;

    [Tooltip("The phrases that will be detected. If left empty, all words will be detected.\nKeywords need to exist in the models dictionary, so some words like \"webview\" are better detected as two more common words \"web view\".")]
    [SerializeField]
    private List<string> KeyPhrases = null;

    [Header("UI")]
    [SerializeField]
    private Text ResultText;

    void Awake()
    {
        VoskASR.OnTranscriptionResult += OnTranscriptionResult;
    }

    void Start()
    {
        if (KeyPhrases.Count > 0)
        {
            for (int i = 0; i < KeyPhrases.Count; i++)
                KeyPhrases[i] = ChineseUtil.ToSimp(KeyPhrases[i]);
        }

        VoskASR.Init(this, ModelName, AutoStart, MaxAlternatives, microphoneIndex, KeyPhrases);
    }

    private void OnTranscriptionResult(string obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj);
#endif
        RecognitionResult resultJson = JsonConvert.DeserializeObject<RecognitionResult>(obj);

        ResultText.text = "";

        foreach (var t in resultJson.alternatives)
            ResultText.text += StringFormatter.SingleSpaceSeparate(ChineseUtil.ToTrad(t.text).Replace("[unk]", "-")) + "\n";
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