using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using TMPro;
using System.Collections;
using UnityEngine.UI;


public class UIAuthenticationTest : MonoBehaviour
{
    /// <summary>
    /// Test if the splash screen automatically transitions to the Sign-In scene after 3 seconds.
    /// </summary>
    /// 
    
    [UnityTest]
    public IEnumerator SplashScreenToSignInPageNaigationTest()
    {
        // Load SplashScreen
        SceneManager.LoadScene("SplashScreen");
        yield return new WaitForSeconds(5f); 

        // Verify that Sign-In scene is loaded
        Assert.AreEqual("SignInPage", SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Test if clicking the "Sign Up" link navigates to the Sign-Up page.
    /// </summary>
    /// 

    [UnityTest]
    public IEnumerator TestSignInLinkNavigatesToWhoAreYouPage()
    {
        SceneManager.LoadScene("SignInPage");
        yield return null; // Wait for scene load

        // Find the "Sign Up" button/link
        Button signUpButton = GameObject.Find("HereButton").GetComponent<Button>();
        Assert.IsNotNull(signUpButton, "Sign Up button not found!");

        // Simulate button click
        signUpButton.onClick.Invoke();
        yield return new WaitForSeconds(1); // Wait for transition

        // Verify that Sign-Up scene is loaded
        Assert.AreEqual("WhoAreYouPage", SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Test if clicking "Next" after entering an email moves to the password entry scene.
    /// </summary>
    /// 

    [UnityTest]
    public IEnumerator SignInPageEmailToPasswordNavigationTest()
    {
        // Load SignInPage Scene
        SceneManager.LoadScene("SignInPage");
        yield return null; // Ensure scene loads

        // Find Email Input Field & Button inside EmailPanel
        GameObject emailPanel = GameObject.Find("Panel/EmailPanel");
        Assert.IsNotNull(emailPanel, "EmailPanel not found!");

        TMP_InputField emailField = emailPanel.transform.Find("EmailField")?.GetComponent<TMP_InputField>();
        Button nextButton = emailPanel.transform.Find("NextBtn")?.GetComponent<Button>();

        Assert.IsNotNull(emailField, "EmailField (TMP) not found!");
        Assert.IsNotNull(nextButton, "Next button not found!");

        // Simulate entering email
        emailField.text = "test@example.com";
        yield return null; // Ensure UI updates

        // Click Next
        nextButton.onClick.Invoke();
        yield return new WaitForSeconds(1); // Wait for transition

        // Find Password Panel
        GameObject passwordPanel = GameObject.Find("Panel/PasswordPanel");
        Assert.IsNotNull(passwordPanel, "Password panel not found!");

        // Check if it's active
        Assert.IsTrue(passwordPanel.activeSelf, "Password panel is not active after clicking Next!");
    }



    /// <summary>
    /// Test if entering a password and clicking "Sign In" navigates to Home page.
    /// </summary>
    /// 

    [UnityTest]
    public IEnumerator SignInToSignUpPageNavigationTest()
    {
        // Load SignInPage Scene
        SceneManager.LoadScene("SignInPage");
        yield return new WaitForSeconds(1.0f);

        // Step 1: Verify EmailPanel is initially active
        GameObject panel = GameObject.Find("Panel");
        Assert.IsNotNull(panel, "Main Panel not found!");

        Transform emailPanelTransform = panel.transform.Find("EmailPanel");
        Assert.IsNotNull(emailPanelTransform, "EmailPanel transform not found!");

        GameObject emailPanel = emailPanelTransform.gameObject;
        Assert.IsNotNull(emailPanel, "EmailPanel not found!");
        Assert.IsTrue(emailPanel.activeInHierarchy, "EmailPanel is not initially active!");

        // Enter email and click Next
        TMP_InputField emailField = emailPanel.transform.Find("EmailField").GetComponent<TMP_InputField>();
        Button nextButton = emailPanel.transform.Find("NextBtn").GetComponent<Button>();

        Assert.IsNotNull(emailField, "Email input field not found!");
        Assert.IsNotNull(nextButton, "Next button not found!");

        emailField.text = "patient2@gmail.com";
        yield return null;
        nextButton.onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        // Step 2: Verify PasswordPanel is now active
        Transform passwordPanelTransform = panel.transform.Find("PasswordPanel");
        Assert.IsNotNull(passwordPanelTransform, "PasswordPanel transform not found!");

        GameObject passwordPanel = passwordPanelTransform.gameObject;
        Assert.IsNotNull(passwordPanel, "PasswordPanel not found!");
        Assert.IsTrue(passwordPanel.activeInHierarchy, "PasswordPanel is not active after clicking Next!");

        // TEST 1: Testing Back button to return to EmailPanel
        Button backButton = passwordPanel.transform.Find("BacktoEmailBtn").GetComponent<Button>();
        Assert.IsNotNull(backButton, "Back button not found!");

        backButton.onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        // Verify EmailPanel is active again
        Assert.IsTrue(emailPanel.activeInHierarchy, "EmailPanel is not active after clicking Back button!");
        Assert.IsFalse(passwordPanel.activeInHierarchy, "PasswordPanel is still active after clicking Back button!");

        // Go back to PasswordPanel for remaining tests
        nextButton.onClick.Invoke();
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(passwordPanel.activeInHierarchy, "PasswordPanel is not active after clicking Next again!");

        // TEST 2: Testing Sign In button navigation to PatientHomePage
        TMP_InputField passwordField = passwordPanel.transform.Find("PasswordField").GetComponent<TMP_InputField>();
        Button signInButton = passwordPanel.transform.Find("SignInBtn").GetComponent<Button>();

        Assert.IsNotNull(passwordField, "Password input field not found!");
        Assert.IsNotNull(signInButton, "Sign In button not found!");

        // Enter password and click Sign In
        passwordField.text = "Asdasd@123";
        yield return null;
        signInButton.onClick.Invoke();
        yield return new WaitForSeconds(5f);

        // Verify PatientHomePage is loaded
        Assert.AreEqual("PatientHomePage", SceneManager.GetActiveScene().name, "Navigation to PatientHomePage failed!");

        // TEST 3: Testing Forgot Password button navigation to ResetPasswordPage
        // Reload SignInPage for Forgot Password test
        SceneManager.LoadScene("SignInPage");
        yield return new WaitForSeconds(1.0f);

        // Navigate through EmailPanel to PasswordPanel again
        panel = GameObject.Find("Panel");
        emailPanel = panel.transform.Find("EmailPanel").gameObject;
        emailField = emailPanel.transform.Find("EmailField").GetComponent<TMP_InputField>();
        nextButton = emailPanel.transform.Find("NextBtn").GetComponent<Button>();

        emailField.text = "patient2@gmail.com";
        yield return null;
        nextButton.onClick.Invoke();
        yield return new WaitForSeconds(5f);

        // Find PasswordPanel again
        passwordPanel = panel.transform.Find("PasswordPanel").gameObject;
        Button forgotPasswordButton = passwordPanel.transform.Find("ForgotPasswordBtn").GetComponent<Button>();
        Assert.IsNotNull(forgotPasswordButton, "Forgot Password button not found!");

        // Click Forgot Password button
        forgotPasswordButton.onClick.Invoke();
        yield return new WaitForSeconds(5f);

        // Verify ResetPasswordPage is loaded
        Assert.AreEqual("ResetPasswordPage", SceneManager.GetActiveScene().name, "Navigation to ResetPasswordPage failed!");
    }


    [UnityTest]
    public IEnumerator TestWhoAreYouPageNavigationTest()
    {
        // Load WhoAreYouPage Scene
        SceneManager.LoadScene("WhoAreYouPage");
        yield return new WaitForSeconds(1.0f); // Wait for scene to load completely

        // Find both buttons on the page
        GameObject patientButton = GameObject.Find("PatientImg");
        Assert.IsNotNull(patientButton, "Patient button not found!");

        GameObject therapistButton = GameObject.Find("TherapistImg");
        Assert.IsNotNull(therapistButton, "Therapist button not found!");

        // TEST 1: Verify PatientImg button navigates to SignUpPage
        Button patientImgButton = patientButton.GetComponent<Button>();
        Assert.IsNotNull(patientImgButton, "Button component not found on PatientImg!");

        // Click the Patient button
        Debug.Log("Clicking PatientImg button");
        patientImgButton.onClick.Invoke();
        yield return new WaitForSeconds(1.0f);

        // Verify navigation to SignUpPage
        Assert.AreEqual("SignUpPage", SceneManager.GetActiveScene().name,
            "Navigation to SignUpPage failed after clicking PatientImg!");

        // Load WhoAreYouPage again for the second test
        SceneManager.LoadScene("WhoAreYouPage");
        yield return new WaitForSeconds(1.0f);

        // TEST 2: Verify TherapistImg button navigates to SignUpPage
        therapistButton = GameObject.Find("TherapistImg");
        Button therapistImgButton = therapistButton.GetComponent<Button>();
        Assert.IsNotNull(therapistImgButton, "Button component not found on TherapistImg!");

        // Click the Therapist button
        Debug.Log("Clicking TherapistImg button");
        therapistImgButton.onClick.Invoke();
        yield return new WaitForSeconds(1.0f);

        // Verify navigation to SignUpPage
        Assert.AreEqual("SignUpPage", SceneManager.GetActiveScene().name,
            "Navigation to SignUpPage failed after clicking TherapistImg!");
    }

}
