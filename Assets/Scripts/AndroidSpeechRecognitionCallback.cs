using UnityEngine;
using System;

#if UNITY_ANDROID
public class AndroidSpeechRecognitionCallback : AndroidJavaProxy
{
    private ArticulationGame game;

    public AndroidSpeechRecognitionCallback(ArticulationGame game) : base("android.speech.RecognitionListener")
    {
        this.game = game;
    }

    public void onReadyForSpeech(AndroidJavaObject bundle) { }

    public void onBeginningOfSpeech() { }

    public void onRmsChanged(float rmsdB) { }

    public void onBufferReceived(byte[] buffer) { }

    public void onEndOfSpeech() { }

    public void onError(int error)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            Debug.LogError("Speech recognition error: " + error);
        });
    }

    public void onResults(AndroidJavaObject results)
    {
        AndroidJavaObject bundleArray = results.Call<AndroidJavaObject>("getStringArrayList", "android.speech.RecognizerIntent.EXTRA_RESULTS");
        string result = bundleArray.Call<string>("get", 0);
        
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            game.OnSpeechResult(result);
        });
    }

    public void onPartialResults(AndroidJavaObject bundle) { }

    public void onEvent(int eventType, AndroidJavaObject bundle) { }
}
#endif

