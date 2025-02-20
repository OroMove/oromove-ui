using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrientationManager : MonoBehaviour
{
    // Singleton instance to avoid duplicates
    private static OrientationManager instance;

    // List of scenes that should be in Landscape mode
    private string[] landscapeScenes = { "HillClimberGameMenu", "HC - Level 1", "HC - Level 2", "HC - Level 3", "HC - Level 4", "HC - Level 5", "HC - Level 6" };

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void OnEnable()
    {
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Stop listening when disabled
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded Scene: " + scene.name); // Debug current scene name

        // Check if the loaded scene is in the landscapeScenes array
        if (System.Array.Exists(landscapeScenes, s => s == scene.name))
        {
            Debug.Log("Switching to Landscape Mode");
            StartCoroutine(SetOrientation(ScreenOrientation.LandscapeLeft)); // Set to Landscape
        }
        else
        {
            Debug.Log("Switching to Portrait Mode");
            StartCoroutine(SetOrientation(ScreenOrientation.Portrait)); // Set to Portrait
        }
    }

    private IEnumerator SetOrientation(ScreenOrientation orientation)
    {
        // Disable auto-rotation temporarily
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;

        // Set the desired orientation
        Screen.orientation = orientation;

        // Wait for the orientation to change
        yield return new WaitForSeconds(0.2f); // Adjust delay if needed

        // Re-enable auto-rotation for allowed orientations
        if (orientation == ScreenOrientation.Portrait)
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
        }
        else
        {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        Debug.Log("Screen Orientation Set To: " + orientation);
    }
}