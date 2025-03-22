using UnityEngine;
using TMPro;

public class ArticulationExercise : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI speechResultText;

    private AndroidTTS tts;
    private AndroidSTT stt;

    private string[] prompts = {
        "The quick brown fox jumps over the lazy dog.",
        "I saw Susie sitting in a shoeshine shop.",
        "Flo rode a boat to the shore.",
        "Irish wristwatch, Swiss wristwatch.",
        "Joe throws a stone to the cove..",
        "She sells seashells by the seashore.",
        "Peter Piper picked a peck of pickled peppers.",
        "Bo knows the golden rose.",
        "Fuzzy Wuzzy was a bear. Fuzzy Wuzzy had no hair.",
        "Red lorry, yellow lorry.",
        "Joe rode a slow boat home..",
        "Betty bought a bit of butter, but the butter was bitter."

    };

    void Start()
    {
        tts = gameObject.AddComponent<AndroidTTS>();
        stt = gameObject.AddComponent<AndroidSTT>();
    }

    public void OnStartButtonClicked()
    {
        // Generate a random prompt
        string randomPrompt = prompts[Random.Range(0, prompts.Length)];
        promptText.text = randomPrompt;
    }

    public void OnMicButtonClicked()
    {
        // Use TTS to read the prompt
        tts.Speak(promptText.text);
    }

    public void OnSpeakButtonClicked()
    {
        // Start STT
        stt.StartListening();
        speechResultText.text = "Voice magic activated!..";
    }
}