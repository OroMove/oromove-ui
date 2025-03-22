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

public enum SignInError
{
    WrongUsernamePassword,
    AccountNotFound,
    ConnectionError,
    UnexpectedError
}

public class SignInController : MonoBehaviour
{
    public TMP_InputField emailInputField, passwordInputField;
    public Button nextButton, signInButton, backButton, forgotPasswordButton, signUpHereButton;
    public GameObject emailPanel, passwordPanel;
    //public GameObject loadingPanel;
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

        //loadingPanel.SetActive(true);
        Debug.Log("Signing in...");
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            Debug.Log("Sign-In successful!");

            await RedirectUserBasedOnRole();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request Failed: " + ex.Message);

            if (ex.Message.Contains("WRONG_USERNAME_PASSWORD"))
            {
                HandleSignInError(SignInError.WrongUsernamePassword);
            }
            else if (ex.Message.Contains("account not found"))
            {
                HandleSignInError(SignInError.AccountNotFound);
            }
            else
            {
                HandleSignInError(SignInError.ConnectionError);
            }
        }

        catch (System.Exception ex)
        {
            Debug.LogError("Unexpected Error: " + ex.Message);
            HandleSignInError(SignInError.UnexpectedError);
        }
        finally
        {
            //loadingPanel.SetActive(false); 
        }

    }

    private const string RoleKey = "role"; // Define a constant key

    async Task RedirectUserBasedOnRole()
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { RoleKey });

            if (data.TryGetValue(RoleKey, out var roleData))
            {
                string role = roleData.Value.GetAsString();
                Debug.Log("User Role Retrieved: " + role);

                string nextScene = role == "Therapist" ? "TherapistEditProfilePage" :
                                  (role == "Patient" ? "PatientHomePage" : null);

                if (!string.IsNullOrEmpty(nextScene))
                {
                    SceneManager.LoadScene(nextScene);
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

    void HandleSignInError(SignInError error)
{
    switch (error)
    {
        case SignInError.WrongUsernamePassword:
            ShowErrorMessage("Incorrect email or password. Please try again.");
            break;
        case SignInError.AccountNotFound:
            ShowErrorMessage("Email not registered. Please sign up.");
            break;
        case SignInError.ConnectionError:
            ShowErrorMessage("Sign-in failed. Check your connection and try again.");
            break;
        case SignInError.UnexpectedError:
        default:
            ShowErrorMessage("An unexpected error occurred. Try again later.");
            break;
    }
}
}