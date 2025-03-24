using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PatientProfileCreationController : MonoBehaviour
{
    // PERSONAL INFO
    public TMP_InputField fullNameInput,
        dobInput,
        contactNumberInput,
        emailInput;
    public TMP_Dropdown genderDropdown;

    // MEDICAL INFO
    public TMP_InputField diagnosisInput,
        therapyStartDateInput,
        therapistNameInput;
    public TMP_Dropdown causeOfImpairmentDropdown;
    public Slider severityLevelSlider;
    public TextMeshProUGUI severityValueText;

    // CAREGIVER DETAILS
    public TMP_InputField caregiverFullNameInput,
        caregiverRelationInput,
        caregiverOccupationInput,
        caregiverContactNumberInput,
        caregiverEmailInput;

    public Button backButton,
        doneButton;
    public TextMeshProUGUI errorText;

    private bool unsavedChanges = false;

    async void Start()
    {
        await InitializeUnityServices();

        backButton.onClick.AddListener(OnBackButtonClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);

        genderDropdown.AddOptions(new List<string> { "Select Gender", "Male", "Female", "Other" });
        genderDropdown.options[0].text = "Select Gender";

        causeOfImpairmentDropdown.AddOptions(
            new List<string>
            {
                "Select Cause",
                "Stroke",
                "Brain Injury",
                "Neurological Disorder",
                "Cerebral Palsy",
                "Other",
            }
        );
        causeOfImpairmentDropdown.options[0].text = "Select Cause";

        severityLevelSlider.onValueChanged.AddListener(UpdateSeverityText);
        UpdateSeverityText(severityLevelSlider.value);
    }

    void UpdateSeverityText(float value)
    {
        string[] severityLabels = { "Mild", "Moderate", "Severe" };
        int index = Mathf.Clamp((int)value, 0, severityLabels.Length - 1);
        severityValueText.text = severityLabels[index];
    }

    void OnBackButtonClicked()
    {
        if (unsavedChanges)
        {
            errorText.text = "Unsaved changes will be lost. Are you sure?";
            return;
        }
        SceneManager.LoadScene("SignUpPage");
    }

    async void OnDoneButtonClicked()
    {
        errorText.text = "";

        if (!IsValidProfile())
            return;

        Debug.Log("Saving Patient Profile...");

        try
        {
            await SavePatientProfile();

            await PatientSubmission(
                fullNameInput.text.Trim(),
                diagnosisInput.text.Trim(),
                severityLevelSlider.value,
                CalculateAge(dobInput.text.Trim()),
                genderDropdown.options[genderDropdown.value].text
            );


            SceneManager.LoadScene("PatientHomePage");
        }
        catch (System.Exception ex)
        {
            errorText.text = "Failed to save profile. Please try again.";
        }
    }

    async Task SavePatientProfile()
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        var profileData = new Dictionary<string, object>
        {
            { "fullName", fullNameInput.text.Trim() },
            { "dob", dobInput.text.Trim() },
            { "gender", genderDropdown.options[genderDropdown.value].text },
            { "contactNumber", contactNumberInput.text.Trim() },
            { "email", emailInput.text.Trim() },
            { "diagnosis", diagnosisInput.text.Trim() },
            { "therapyStartDate", therapyStartDateInput.text.Trim() },
            { "therapistName", therapistNameInput.text.Trim() },
            {
                "causeOfImpairment",
                causeOfImpairmentDropdown.options[causeOfImpairmentDropdown.value].text
            },
            { "severityLevel", severityLevelSlider.value },
            { "caregiverFullName", caregiverFullNameInput.text.Trim() },
            { "caregiverRelation", caregiverRelationInput.text.Trim() },
            { "caregiverOccupation", caregiverOccupationInput.text.Trim() },
            { "caregiverContactNumber", caregiverContactNumberInput.text.Trim() },
            { "caregiverEmail", caregiverEmailInput.text.Trim() },
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(
            new Dictionary<string, object> { { "PatientProfile", profileData } }
        );
    }

    public async Task PatientSubmission(string fullName, string diagnosis, float severityLevel, int age, string gender)
    {
        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(diagnosis) || string.IsNullOrEmpty(gender))
        {
            Debug.LogError("Error: Name, Diagnosis, or Gender is empty! Data will not be submitted.");
            return;
        }

        var options = new AddPlayerScoreOptions
        {
            Metadata = new Dictionary<string, string>
        {
            { "Name", fullName },
            { "Diagnosis", diagnosis },
            { "SeverityLevel", severityLevel.ToString() },
            { "Age", age.ToString() },
            { "Gender", gender }
        },
        };

        Debug.Log($"Submitting to leaderboard: Name={fullName}, Diagnosis={diagnosis}, SeverityLevel={severityLevel}, Age={age}, Gender={gender}");

        try
        {
            var result = await LeaderboardsService.Instance.AddPlayerScoreAsync(
                "PatientLeaderboard",
                Mathf.RoundToInt(severityLevel), // Using severity level as the score
                options
            );

            Debug.Log($"Successfully submitted! Stored Metadata: {JsonConvert.SerializeObject(options.Metadata)}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to submit leaderboard entry: {ex.Message}");
        }
    }



    async Task InitializeUnityServices()
    {
        if (
            !Unity.Services.Core.UnityServices.State.Equals(
                Unity.Services.Core.ServicesInitializationState.Initialized
            )
        )
        {
            try
            {
                await Unity.Services.Core.UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Sign in the user
                Debug.Log("Unity Services Initialized Successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to initialize Unity Services: " + ex.Message);
            }
        }
    }

    private int CalculateAge(string dob)
    {
        if (DateTime.TryParse(dob, out DateTime birthDate))
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now < birthDate.AddYears(age))
                age--; // Adjust if birthday hasn't occurred yet this year
            return age;
        }
        Debug.LogError("Invalid DOB format.");
        return 0;
    }

    bool IsValidProfile()
    {
        List<string> errors = new List<string>();

        if (!ValidateName(fullNameInput.text))
            errors.Add("Invalid Full Name.");
        if (string.IsNullOrEmpty(dobInput.text))
            errors.Add("Date of Birth cannot be empty.");
        if (genderDropdown.value == 0)
            errors.Add("Please select a gender.");
        if (!ValidateContactNumber(contactNumberInput.text))
            errors.Add("Invalid Contact Number.");
        if (!ValidateEmail(emailInput.text))
            errors.Add("Invalid Email Format.");
        if (string.IsNullOrEmpty(diagnosisInput.text))
            errors.Add("Diagnosis cannot be empty.");
        if (string.IsNullOrEmpty(therapyStartDateInput.text))
            errors.Add("Therapy Start Date cannot be empty.");
        if (causeOfImpairmentDropdown.value == 0)
            errors.Add("Please select a cause of impairment.");
        if (!ValidateName(caregiverFullNameInput.text))
            errors.Add("Invalid Caregiver Full Name.");
        if (string.IsNullOrEmpty(caregiverRelationInput.text))
            errors.Add("Relation to Patient cannot be empty.");
        if (!ValidateContactNumber(caregiverContactNumberInput.text))
            errors.Add("Invalid Caregiver Contact Number.");
        if (!ValidateEmail(caregiverEmailInput.text))
            errors.Add("Invalid Caregiver Email.");

        if (errors.Count > 0)
        {
            errorText.text = string.Join("\n", errors);
            return false;
        }

        return true;
    }

    bool ValidateName(string name) => Regex.IsMatch(name, "^[a-zA-Z ]+$");
    bool ValidateContactNumber(string number) => Regex.IsMatch(number, @"^0\d{9}$");


    bool ValidateEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
