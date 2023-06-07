using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoudnessMeter : MonoBehaviour
{
    private const int sample = 32;
    private const float threshold = 0.01f;
    private const float scale = 2.5f;

    private int meterCount;
    private Image[] meters;
    private float[] history;

    void Start()
    {
        Application.targetFrameRate = 60;
        meterCount = transform.childCount;

        meters = new Image[meterCount];
        for (int i = 0; i < meterCount; i++)
            meters[i] = transform.GetChild(i).GetComponent<Image>();

        List<float> _ = new List<float>();
        for (int i = 0; i < meterCount; i++)
            _.Add(0);

        history = _.ToArray();
    }

    void Update()
    {
        for (int i = 0; i < meterCount - 1; i++)
            history[i] = history[i + 1];
        history[meterCount - 1] = VoiceProcessor.IsRecording ? GetLoudnessFromClip(Microphone.GetPosition(VoiceProcessor.CurrentDeviceName()), VoiceProcessor.GetClip()) : 0.0f;

        for (int i = 0; i < meterCount; i++)
            meters[i].rectTransform.localScale = new Vector3(1, history[i], 1);
    }

    private float GetLoudnessFromClip(int position, AudioClip clip)
    {
        int startPosition = position - sample;
        if (startPosition < 0)
            return 0;

        float[] data = new float[sample];
        clip.GetData(data, startPosition);

        float total = 0.0f;

        for (int i = 0; i < sample; i++)
            total += Mathf.Abs(data[i]);

        float v = total / sample;

        if (v < threshold)
            return 0.1f;
        else
            return Mathf.Lerp(0.1f, 2.0f, v * scale);
    }
}