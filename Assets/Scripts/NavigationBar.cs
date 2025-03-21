using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavigationBar : MonoBehaviour
{
    // Assign these in the Inspector
    public Button homeButton;
    public Button settingsButton;
    public Button progressButton;
    public Button gamesButton;

    void Start()
    {
        // Add listener to each button
        homeButton.onClick.AddListener(() => LoadScene("PatientHomePage"));
        settingsButton.onClick.AddListener(() => LoadScene("PatientSettingsPage"));
        progressButton.onClick.AddListener(() => LoadScene("PatientHomePage"));
        gamesButton.onClick.AddListener(() => LoadScene("GameLibPage"));
    }

    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

