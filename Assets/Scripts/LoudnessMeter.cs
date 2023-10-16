using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple script to show if Microphone is working
/// </summary>
public class LoudnessMeter : MonoBehaviour
{
    private const int sample = 16;
    private const float interval = 1.0f / 60;
    private const float scale = 3.2f;

    private int meterCount;
    private Image[] meters;
    private Queue<float> history;
    private static float[] buffer;

    void Awake()
    {
        meterCount = transform.childCount;

        meters = new Image[meterCount];
        for (int i = 0; i < meterCount; i++)
            meters[i] = transform.GetChild(i).GetComponent<Image>();

        history = new Queue<float>();
        buffer = new float[sample];

        for (int i = 0; i < meterCount; i++)
            history.Enqueue(0.0f);

        StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            history.Dequeue();

            history.Enqueue(VoiceProcessor.IsRecording ? GetLoudnessFromClip(Microphone.GetPosition(VoiceProcessor.CurrentDeviceName), VoiceProcessor.GetClip()) : 0.0f);

            int i = 0;
            foreach (float loudness in history)
            {
                meters[i].rectTransform.localScale = new Vector3(1.0f, loudness * scale, 1.0f);
                i++;
            }
        }
    }

    private static float GetLoudnessFromClip(int position, AudioClip clip)
    {
        int startPosition = position - sample;
        if (startPosition < 0)
            return 0;

        clip.GetData(buffer, startPosition);

        float total = 0.0f;

        for (int i = 0; i < sample; i++)
            total += Mathf.Abs(buffer[i]);

        return total / sample;
    }
}
