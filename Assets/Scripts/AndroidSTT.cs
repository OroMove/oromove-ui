using UnityEngine;
using UnityEngine.Android;
using System.Collections; // Add this namespace for IEnumerator

public class AndroidSTT : MonoBehaviour
{
    private AndroidJavaObject speechRecognizer;
    private bool isInitialized = false;

    void Start()
    {
        // Request microphone permission
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.Log("Requesting microphone permission.");
            Permission.RequestUserPermission(Permission.Microphone);
            StartCoroutine(CheckMicrophonePermission());
        }
        else
        {
            Debug.Log("Microphone permission already granted.");
            InitializeSTT();
        }
    }

    IEnumerator CheckMicrophonePermission()
    {
        yield return new WaitForSeconds(1); // Wait for 1 second
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.Log("Microphone permission granted.");
            InitializeSTT();
        }
        else
        {
            Debug.LogError("Microphone permission not granted.");
        }
    }

    void InitializeSTT()
    {
        try
        {
            // Initialize STT
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            speechRecognizer = new AndroidJavaObject("android.speech.SpeechRecognizer", activity);
            speechRecognizer.Call("setRecognitionListener", new STTListener());
            isInitialized = true;
            Debug.Log("SpeechRecognizer initialized successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize SpeechRecognizer: " + e.Message);
        }
    }

    public void StartListening()
    {
        if (!isInitialized)
        {
            Debug.LogError("SpeechRecognizer is not initialized.");
            return;
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.LogError("Microphone permission not granted.");
            return;
        }

        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
        intent.Call<AndroidJavaObject>("setAction", "android.speech.action.RECOGNIZE_SPEECH");
        speechRecognizer.Call("startListening", intent);
        Debug.Log("Started listening...");
    }

    class STTListener : AndroidJavaProxy
    {
        public STTListener() : base("android.speech.RecognitionListener") { }

        void onResults(AndroidJavaObject results)
        {
            AndroidJavaObject matches = results.Call<AndroidJavaObject>("getStringArrayList", "results_recognition");
            string text = matches.Call<string>("get", 0);
            Debug.Log("Recognized: " + text);
            // Update the speech result text in the UI
            GameObject.Find("SpeechResultText").GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }

        void onError(int error)
        {
            Debug.LogError("Speech recognition error: " + error);
        }
    }
}