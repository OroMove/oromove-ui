using UnityEngine;
using TMPro;
using UnityEngine.UI;  // For Button functionality
using UnityEngine.SceneManagement;  // To load scenes

public class ResetPasswordManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public Button submitButton;
    public Button backButton;
    public Button backToLoginButton;

    void Start()
    {
        // Add listeners for buttons
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        backToLoginButton.onClick.AddListener(OnBackToLoginButtonClicked);
    }

    // Called when the submit button is clicked
    void OnSubmitButtonClicked()
    {
        string email = emailInputField.text;

        // Example reset password logic (this can be replaced with actual reset password logic)
        if (IsValidEmail(email))
        {
            Debug.Log("Reset link sent to: " + email);
            // After sending the reset link, you might want to show a message or go back to the login page
            SceneManager.LoadScene("SignInPage");  // Replace with your actual Login scene name
        }
        else
        {
            Debug.Log("Invalid email.");
            // You can show an error message here (e.g., using a text field or a popup)
        }
    }

    // Called when the back button is clicked (to go back to the login page)
    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("SignInPage");  // Replace with your actual Login scene name
    }

    // Called when the "Back to Login" button is clicked
    void OnBackToLoginButtonClicked()
    {
        SceneManager.LoadScene("SignInPage");  // Replace with your actual Login scene name
    }

    // Example of email validation (this can be replaced with more complex validation)
    bool IsValidEmail(string email)
    {
        // Simple email validation (just for demonstration)
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }
}
