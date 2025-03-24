using UnityEngine;

public class AndroidTTS : MonoBehaviour
{
    private AndroidJavaObject tts;

    void Start()
    {
        // Initialize TTS
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TTSListener());
    }

    public void Speak(string text)
    {
        if (tts != null)
        {
            AndroidJavaObject paramsObj = new AndroidJavaObject("android.os.Bundle");
            tts.Call<int>("speak", text, 0, paramsObj, "utteranceId");
        }
    }

    class TTSListener : AndroidJavaProxy
    {
        public TTSListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }
        void OnInit(int status)
        {
            Debug.Log("TTS Initialized with status: " + status);
        }
    }
}