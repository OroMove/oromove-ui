using UnityEngine;
using System;

#if UNITY_ANDROID
public class AndroidSpeechTTSCallback : AndroidJavaProxy
{
    public AndroidSpeechTTSCallback() : base("android.speech.tts.TextToSpeech$OnInitListener")
    {
    }

    public void onInit(int status)
    {
        if (status == 0) // SUCCESS
        {
            Debug.Log("TTS initialized successfully");
        }
        else
        {
            Debug.LogError("Failed to initialize TTS");
        }
    }
}
#endif

