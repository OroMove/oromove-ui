using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SignInController : MonoBehaviour
{
    public TMP_InputField emailInputField, passwordInputField;
    public Button nextButton, signInButton, backButton, forgotPasswordButton, signUpHereButton;
    public GameObject emailPanel, passwordPanel;
    public TextMeshProUGUI errorText;

    private string email;

    void Start()
    {
        // Initialize Unity Services
        InitializeUnityServices();

        nextButton.onClick.AddListener(OnNextButtonClicked);
        signInButton.onClick.AddListener(OnSignInButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordButtonClicked);
        signUpHereButton.onClick.AddListener(OnSignUpHereButtonClicked);

        emailPanel.SetActive(true);
        passwordPanel.SetActive(false);
        errorText.gameObject.SetActive(false);
    }

    // Initialize Unity Services asynchronously
    async void InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services: " + e.Message);
            ShowErrorMessage("Failed to connect to server. Try again later.");
        }
    }

    // Called when "Next" is clicked
    void OnNextButtonClicked()
    {
        email = emailInputField.text;

        if (IsValidEmail(email))
        {
            // Proceed to password panel
            emailPanel.SetActive(false);
            passwordPanel.SetActive(true);
            errorText.gameObject.SetActive(false);
        }
        else
        {
            ShowErrorMessage("Invalid email format. Please enter a valid email.");
        }
    }

    async void OnSignInButtonClicked()
    {
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(password))
        {
            ShowErrorMessage("Password cannot be empty.");
            return;
        }

        Debug.Log("Signing in...");
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            Debug.Log("Sign-In successful!");

            // Proceed to Home Page after successful login
            await RedirectUserBasedOnRole();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request Failed: " + ex.Message);

            if (ex.Message.Contains("WRONG_USERNAME_PASSWORD"))
            {
                ShowErrorMessage("Incorrect email or password. Please try again.");
            }
            else if (ex.Message.Contains("account not found"))
            {
                ShowErrorMessage("Email not registered. Please sign up.");
            }
            else
            {
                ShowErrorMessage("Sign-in failed. Check your connection and try again.");
            }
        }
        
        catch (System.Exception ex)
        {
            Debug.LogError("Unexpected Error: " + ex.Message);
            ShowErrorMessage("An unexpected error occurred. Try again later.");
        }
    }

    async Task RedirectUserBasedOnRole()
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("Fetching role for Player ID: " + playerId);

            ISet<string> keys = new HashSet<string> { "role" };
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (data.TryGetValue("role", out var roleData))
            {
                string role = roleData.Value.GetAsString();
                Debug.Log("User Role Retrieved: " + role);

                if (role == "Therapist")
                {
                    SceneManager.LoadScene("TherapistHomePage");
                }
                else if (role == "Patient")
                {
                    SceneManager.LoadScene("PatientHomePage");
                }
                else
                {
                    ShowErrorMessage("No account found. Please sign up.");
                }
            }
            else
            {
                ShowErrorMessage("No account found. Please sign up.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching role: " + ex.Message);
            ShowErrorMessage("Failed to retrieve user data. Try again later.");
        }
    }





    void OnBackButtonClicked()
    {
        passwordPanel.SetActive(false);
        emailPanel.SetActive(true);
        errorText.gameObject.SetActive(false);
    }

    void OnForgotPasswordButtonClicked()
    {
        SceneManager.LoadScene("ResetPasswordPage");
    }

    void OnSignUpHereButtonClicked()
    {
        SceneManager.LoadScene("WhoAreYouPage");
    }

    bool IsValidEmail(string email)
    {
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    void ShowErrorMessage(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }
}