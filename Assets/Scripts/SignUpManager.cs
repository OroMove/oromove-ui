using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class SignUpManager : MonoBehaviour
{
    public GameObject emailPanel, passwordPanel, confirmPasswordPanel;
    public TMP_InputField emailInput, passwordInput, confirmPasswordInput;
    public Button nextEmailButton, nextPasswordButton, signUpButton, signInButton;
    public Button backToEmailButton, backToPasswordButton, backToRoleButton;
    public TextMeshProUGUI emailErrorText, passwordErrorText, confirmPasswordErrorText;

    private int currentStep = 0;
    private string userRole;

    async void Start()
    {
        await InitializeUGS();

        userRole = RoleSelectionManager.selectedRole;
        Debug.Log("User Role: " + userRole);

        ShowPanel(0);

        // Navigation Buttons
        nextEmailButton.onClick.AddListener(() => NextStep(0));
        nextPasswordButton.onClick.AddListener(() => NextStep(1));
        signUpButton.onClick.AddListener(SignUp);
        signInButton.onClick.AddListener(GoToSignIn);

        // Back Buttons
        backToEmailButton.onClick.AddListener(() => PreviousStep(0));  //Password Panel
        backToPasswordButton.onClick.AddListener(() => PreviousStep(1));  //Email Panel
        backToRoleButton.onClick.AddListener(GoBackToWhoAreYou);
    }

    async Task InitializeUGS()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
    }

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (currentStep == 0) NextStep(0);
            else if (currentStep == 1) NextStep(1);
            else if (currentStep == 2) SignUp();
        }
    }

    void ShowPanel(int step)
    {
        // Activate the correct panel
        emailPanel.SetActive(step == 0);
        passwordPanel.SetActive(step == 1);
        confirmPasswordPanel.SetActive(step == 2);

        // Automatically focus on the correct input field
        if (step == 0) EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
        if (step == 1) EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
        if (step == 2) EventSystem.current.SetSelectedGameObject(confirmPasswordInput.gameObject);
    }

    void NextStep(int step)
    {
        if (step == 0) // Email validation
        {
            if (!IsValidEmail(emailInput.text))
            {
                emailErrorText.text = "Invalid email address!";
                return;
            }
            emailErrorText.text = "";
        }
        else if (step == 1) // Password validation
        {
            if (!IsValidPassword(passwordInput.text))
            {
                passwordErrorText.text = "Password must be 8-30 characters with at least 1 uppercase, 1 lowercase, 1 digit, and 1 symbol!";
                return;
            }
            passwordErrorText.text = "";
        }

        currentStep++;
        ShowPanel(currentStep);
    }

    void PreviousStep(int step)
    {
        currentStep = step;
        ShowPanel(currentStep);
    }

    async void SignUp()
    {
        if (confirmPasswordInput.text != passwordInput.text)
        {
            confirmPasswordErrorText.text = "Passwords do not match!";
            return;
        }
        confirmPasswordErrorText.text = "";

        Debug.Log("Signing Up...");

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(emailInput.text, passwordInput.text);
            Debug.Log("Sign Up Successful!");


            await SaveUserRoleToCloud(userRole);
            // Redirect based on stored role
            if (userRole == "Patient")
            {
                SceneManager.LoadScene("PatientProfileCreationPage");
            }
            else if (userRole == "Therapist")
            {
                SceneManager.LoadScene("TherapistProfileCreationPage");
            }
        }
        catch (AuthenticationException ex)
        {
            // Handle authentication-specific errors
            if (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                emailErrorText.text = "This account already existsS.";
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.InvalidParameters)
            {
                emailErrorText.text = "Invalid input parameters. Please check your email and password.";
            }
            else
            {
                emailErrorText.text = "Authentication failed. Please try again later.";
            }

            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Handle general request failed errors
            emailErrorText.text = "Request failed. Please check your connection and try again.";

            // Optionally, you can check for specific error codes in the RequestFailedException
            if (ex.ErrorCode == AuthenticationErrorCodes.InvalidSessionToken)
            {
                emailErrorText.text = "Session expired. Please log in again.";
            }
            else if (ex.ErrorCode == AuthenticationErrorCodes.ClientNoActiveSession)
            {
                emailErrorText.text = "No active session found. Please log in first.";
            }

            Debug.LogException(ex);
        }
    }

    // Save role to Unity Cloud Save
    async Task SaveUserRoleToCloud(string role)
    {
        try
        {
            Dictionary<string, object> data = new Dictionary<string, object> { { "role", role } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("User role saved successfully: " + role);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save user role: " + ex.Message);
        }
    }

    void GoToSignIn()
    {
        Debug.Log("Redirecting to Sign In...");
        SceneManager.LoadScene("SignInPage");
    }

    void GoBackToWhoAreYou()
    {
        Debug.Log("Going back to WhoAreYou...");
        SceneManager.LoadScene("WhoAreYouPage");
    }

    bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    bool IsValidPassword(string password)
    {
        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,30}$";
        return Regex.IsMatch(password, passwordPattern);
    }

}
