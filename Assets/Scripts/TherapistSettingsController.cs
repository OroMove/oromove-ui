using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Use Unity's UI system for Canvas-based buttons
using Unity.Services.Authentication;

public class TherapistSettingsController : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public Button backButton;
    public Button editProfileButton;
    public Button logOutButton;

    private void Start()
    {
        // Assign button click events
        backButton.onClick.AddListener(GoToHomePage);
        editProfileButton.onClick.AddListener(GoToEditProfilePage);
        logOutButton.onClick.AddListener(Logout);
    }

    // Function to go back to Home Page
    private void GoToHomePage()
    {
        SceneManager.LoadScene("TherapistHomePage");
    }

    // Function to go to Edit Profile Page
    private void GoToEditProfilePage()
    {
        SceneManager.LoadScene("TherapistEditProfilePage");
    }

    // Function to handle logout
    private void Logout()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                AuthenticationService.Instance.SignOut(true); // True clears all credentials
                Debug.Log("User signed out successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Logout failed: {e.Message}");
            }
        }

        // Redirect to Login Page
        SceneManager.LoadScene("SignInPage");
    }
}
