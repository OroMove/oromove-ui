using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Linq;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class ArticulationGame : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI transcriptText;
    public TextMeshProUGUI accuracyText;
    public Button playButton;
    public Button readButton;
    public Button speakButton;
    public Image micAnimation;

    [Header("Prompts")]
    [TextArea(3, 5)]
    public string[] prompts = {
        "The quick brown fox jumps over the lazy dog.",
        "She sells seashells by the seashore.",
        "How can a clam cram in a clean cream can?",
        "Peter Piper picked a peck of pickled peppers.",
        "Unique New York, unique New York, you need unique New York."
    };

    private string currentPrompt;
    private bool isRecording = false;
    private AudioClip recordedClip;
    private AndroidJavaObject speechRecognizer;
    private AndroidJavaObject textToSpeech;
    
    // For audio playback
    private AudioSource audioSource;

    void Start()
    {
        // Initialize audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Assign button listeners
        playButton.onClick.AddListener(GeneratePrompt);
        readButton.onClick.AddListener(ReadPrompt);
        speakButton.onClick.AddListener(ToggleSpeechRecognition);

        // Initialize UI
        promptText.text = "Press Play to generate a prompt";
        transcriptText.text = "";
        accuracyText.text = "";
        micAnimation.color = Color.gray;
        
        // Check for microphone permissions on Android
        CheckMicrophonePermission();
        
        // Initialize Android speech services
        #if UNITY_ANDROID && !UNITY_EDITOR
        InitializeAndroidSpeechServices();
        #endif
    }

    void CheckMicrophonePermission()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
        #endif
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    void InitializeAndroidSpeechServices()
    {
        try
        {
            // Initialize Android Text-to-Speech
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Create Text-to-Speech instance
                    textToSpeech = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new AndroidSpeechTTSCallback());
                    
                    // Create Speech Recognizer
                    using (AndroidJavaClass speechRecognizerClass = new AndroidJavaClass("android.speech.SpeechRecognizer"))
                    {
                        if (speechRecognizerClass.CallStatic<bool>("isRecognitionAvailable", activity))
                        {
                            speechRecognizer = speechRecognizerClass.CallStatic<AndroidJavaObject>("createSpeechRecognizer", activity);
                            speechRecognizer.Call("setRecognitionListener", new AndroidSpeechRecognitionCallback(this));
                        }
                        else
                        {
                            Debug.LogError("Speech recognition not available on this device");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error initializing Android speech services: " + e.Message);
        }
    }
    #endif

    void GeneratePrompt()
    {
        currentPrompt = prompts[UnityEngine.Random.Range(0, prompts.Length)];
        promptText.text = currentPrompt;
        transcriptText.text = "Speak the prompt shown above...";
        accuracyText.text = "";
    }

    void ReadPrompt()
    {
        if (string.IsNullOrEmpty(currentPrompt))
        {
            Debug.LogWarning("No prompt to read. Generate a prompt first.");
            return;
        }

        #if UNITY_EDITOR
        // In editor, just log the text
        Debug.Log("Reading: " + currentPrompt);
        StartCoroutine(SimulateSpeaking(currentPrompt));
        #elif UNITY_STANDALONE_WIN
        // On Windows, use Windows TTS
        try {
            System.Speech.Synthesis.SpeechSynthesizer synth = new System.Speech.Synthesis.SpeechSynthesizer();
            synth.SpeakAsync(currentPrompt);
        }
        catch (Exception e) {
            Debug.LogError("Windows TTS error: " + e.Message);
        }
        #elif UNITY_ANDROID
        // On Android, use Android TTS
        if (textToSpeech != null)
        {
            textToSpeech.Call("speak", currentPrompt, 0, null);
        }
        #else
        // On other platforms, simulate speaking
        StartCoroutine(SimulateSpeaking(currentPrompt));
        #endif
    }

    IEnumerator SimulateSpeaking(string text)
    {
        readButton.interactable = false;
        yield return new WaitForSeconds(text.Length * 0.05f); // Simulate speaking time
        readButton.interactable = true;
    }

    void ToggleSpeechRecognition()
    {
        if (string.IsNullOrEmpty(currentPrompt))
        {
            Debug.LogWarning("No prompt to compare against. Generate a prompt first.");
            return;
        }

        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    void StartRecording()
    {
        #if UNITY_EDITOR
        // In editor, simulate recording
        isRecording = true;
        micAnimation.color = Color.red;
        transcriptText.text = "Listening...";
        StartCoroutine(SimulateRecording());
        #elif UNITY_STANDALONE_WIN
        // On Windows, use Windows speech recognition
        try {
            var dictationRecognizer = new UnityEngine.Windows.Speech.DictationRecognizer();
            dictationRecognizer.DictationResult += (text, confidence) => {
                OnSpeechResult(text);
                dictationRecognizer.Stop();
                dictationRecognizer.Dispose();
            };
            dictationRecognizer.Start();
            isRecording = true;
            micAnimation.color = Color.red;
            transcriptText.text = "Listening...";
        }
        catch (Exception e) {
            Debug.LogError("Windows speech recognition error: " + e.Message);
        }
        #elif UNITY_ANDROID
        // On Android, use Android speech recognition
        if (speechRecognizer != null)
        {
            try
            {
                using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.speech.action.RECOGNIZE_SPEECH"))
                {
                    intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE_MODEL", "free_form");
                    intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.MAX_RESULTS", 1);
                    intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE", "en-US");
                    
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                        {
                            speechRecognizer.Call("startListening", intent);
                            isRecording = true;
                            micAnimation.color = Color.red;
                            transcriptText.text = "Listening...";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Android speech recognition error: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Speech recognizer not initialized");
            // Fall back to simulation
            StartCoroutine(SimulateRecording());
        }
        #else
        // On other platforms, simulate recording
        StartCoroutine(SimulateRecording());
        #endif
    }

    void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        micAnimation.color = Color.gray;
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (speechRecognizer != null)
        {
            speechRecognizer.Call("stopListening");
        }
        #endif
    }

    IEnumerator SimulateRecording()
    {
        // Wait for a simulated recording time
        yield return new WaitForSeconds(2.0f);
        
        // Create a simulated result with some errors
        System.Random random = new System.Random();
        string[] words = currentPrompt.Split(' ');
        
        // Simulate some recognition errors
        for (int i = 0; i < words.Length; i++)
        {
            if (random.NextDouble() < 0.2f) // 20% chance of word error
            {
                // Either skip a word, replace it, or mangle it
                double errorType = random.NextDouble();
                if (errorType < 0.3f)
                {
                    words[i] = ""; // Skip word
                }
                else if (errorType < 0.6f && words[i].Length > 3)
                {
                    // Mangle a word by changing a character
                    char[] chars = words[i].ToCharArray();
                    int pos = random.Next(chars.Length);
                    chars[pos] = (char)('a' + random.Next(26));
                    words[i] = new string(chars);
                }
                // else keep the word (simulate correct recognition)
            }
        }
        
        string simulatedText = string.Join(" ", words.Where(w => !string.IsNullOrEmpty(w)));
        OnSpeechResult(simulatedText);
        
        StopRecording();
    }

    public void OnSpeechResult(string result)
    {
        transcriptText.text = result;
        float accuracy = CalculateAccuracy(result, currentPrompt);
        accuracyText.text = $"Accuracy: {accuracy:F1}%";
    }

    float CalculateAccuracy(string spoken, string expected)
    {
        // Improved accuracy calculation using Levenshtein distance
        
        // Normalize strings: remove punctuation, convert to lowercase
        spoken = NormalizeString(spoken);
        expected = NormalizeString(expected);
        
        string[] spokenWords = spoken.Split(' ');
        string[] expectedWords = expected.Split(' ');
        
        // If nothing was spoken
        if (spokenWords.Length == 0)
            return 0;
            
        int correctWords = 0;
        
        // Count exact matches
        for (int i = 0; i < Math.Min(spokenWords.Length, expectedWords.Length); i++)
        {
            if (i < spokenWords.Length && i < expectedWords.Length)
            {
                if (spokenWords[i] == expectedWords[i])
                {
                    correctWords++;
                }
                else
                {
                    // Check for similarity using Levenshtein distance
                    float similarity = 1.0f - (float)LevenshteinDistance(spokenWords[i], expectedWords[i]) / 
                                      Math.Max(spokenWords[i].Length, expectedWords[i].Length);
                    
                    if (similarity > 0.7f) // If words are at least 70% similar
                    {
                        correctWords += (int)(similarity * 100) / 100; // Partial credit
                    }
                }
            }
        }
        
        // Calculate final accuracy
        float wordsAccuracy = (float)correctWords / expectedWords.Length * 100;
        
        // Penalize for extra or missing words
        float lengthPenalty = 1.0f - Math.Abs(spokenWords.Length - expectedWords.Length) / 
                             (float)Math.Max(expectedWords.Length, 1) * 0.5f; // 50% max penalty
        
        return Math.Max(0, wordsAccuracy * Math.Max(0.5f, lengthPenalty));
    }
    
    string NormalizeString(string input)
    {
        // Remove punctuation and convert to lowercase
        return new string(input.ToLower()
            .Where(c => !char.IsPunctuation(c))
            .ToArray())
            .Trim();
    }
    
    int LevenshteinDistance(string s, string t)
    {
        // Compute Levenshtein distance between two strings
        
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        
        if (n == 0) return m;
        if (m == 0) return n;
        
        for (int i = 0; i <= n; i++)
            d[i, 0] = i;
            
        for (int j = 0; j <= m; j++)
            d[0, j] = j;
            
        for (int j = 1; j <= m; j++)
        {
            for (int i = 1; i <= n; i++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        
        return d[n, m];
    }

    void OnDestroy()
    {
        // Clean up resources
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (speechRecognizer != null)
        {
            speechRecognizer.Call("destroy");
            speechRecognizer.Dispose();
        }
        
        if (textToSpeech != null)
        {
            textToSpeech.Call("shutdown");
            textToSpeech.Dispose();
        }
        #endif
    }
}

