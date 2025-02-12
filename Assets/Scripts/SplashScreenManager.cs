using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreenManager : MonoBehaviour
{
    public float splashScreenDuration = 3f; // Duration of the splash screen in seconds
    public string nextSceneName = "HomePage"; // Name of the next scene to load
    public Image fadePanel; // Reference to the fade panel
    public float fadeDuration = 1f; // Duration of the fade effect

    void Start()
    {
        // Start the coroutine to wait, fade out, and load the next scene
        StartCoroutine(WaitAndLoadNextScene());
    }

    System.Collections.IEnumerator WaitAndLoadNextScene()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(splashScreenDuration);

        // Fade out
        float elapsedTime = 0f;
        Color panelColor = fadePanel.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            yield return null;
        }

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}