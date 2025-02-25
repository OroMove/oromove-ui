using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class PatientProfileCreationController : MonoBehaviour
{
    // PERSONAL INFO
    public TMP_InputField fullNameInput;
    public TMP_InputField dobInput;
    public TMP_Dropdown genderDropdown;
    public TMP_InputField contactNumberInput;
    public TMP_InputField emailInput;

    // MEDICAL INFO
    public TMP_InputField diagnosisInput;
    public TMP_InputField therapyStartDateInput;
    public TMP_InputField therapistNameInput; 
    public TMP_Dropdown causeOfImpairmentDropdown; 
    public Slider severityLevelSlider;
    public TextMeshProUGUI severityValueText;

    // CAREGIVER DETAILS
    public TMP_InputField caregiverFullNameInput;
    public TMP_InputField caregiverRelationInput;
    public TMP_InputField caregiverOccupationInput;
    public TMP_InputField caregiverContactNumberInput;
    public TMP_InputField caregiverEmailInput;

    public Button backButton;
    public Button doneButton;
    public TextMeshProUGUI errorText;

    private bool unsavedChanges = false;

    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);

        // Populate gender dropdown
        genderDropdown.ClearOptions();
        genderDropdown.AddOptions(new List<string> { "Select Gender", "Male", "Female", "Other" });

        // Populate cause of impairment dropdown
        causeOfImpairmentDropdown.ClearOptions();
        causeOfImpairmentDropdown.AddOptions(new List<string>
        {
            "Select Cause", "Stroke", "Brain Injury", "Neurological Disorder", "Cerebral Palsy", "Other"
        });

        UpdateSeverityText(severityLevelSlider.value);
        severityLevelSlider.onValueChanged.AddListener(UpdateSeverityText);

        fullNameInput.onValueChanged.AddListener(delegate { unsavedChanges = true; ValidateName(fullNameInput.text); });
        emailInput.onValueChanged.AddListener(delegate { unsavedChanges = true; ValidateEmail(emailInput.text); });

        severityValueText.text = severityLevelSlider.value.ToString("0.0");
        severityLevelSlider.onValueChanged.AddListener(UpdateSeverityText);
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
            Debug.Log("Are you sure you want to go back? Unsaved changes will be lost.");
            return; 
        }
        SceneManager.LoadScene("SignUpPage");
    }

    async void OnDoneButtonClicked()
    {
        string fullName = fullNameInput.text.Trim();
        string dob = dobInput.text.Trim();
        string gender = genderDropdown.options[genderDropdown.value].text;
        string contactNumber = contactNumberInput.text.Trim();
        string email = emailInput.text.Trim();

        string diagnosis = diagnosisInput.text.Trim();
        string therapyStartDate = therapyStartDateInput.text.Trim();
        string therapistName = therapistNameInput.text.Trim(); 
        string causeOfImpairment = causeOfImpairmentDropdown.options[causeOfImpairmentDropdown.value].text; 
        float severityLevel = severityLevelSlider.value;

        string caregiverFullName = caregiverFullNameInput.text.Trim();
        string caregiverRelation = caregiverRelationInput.text.Trim();
        string caregiverOccupation = caregiverOccupationInput.text.Trim();
        string caregiverContactNumber = caregiverContactNumberInput.text.Trim();
        string caregiverEmail = caregiverEmailInput.text.Trim();

        if (IsValidProfile(fullName, dob, gender, contactNumber, email, diagnosis, therapyStartDate, therapistName, causeOfImpairment, caregiverFullName, caregiverRelation, caregiverOccupation, caregiverContactNumber, caregiverEmail))
        {
            Debug.Log("Saving Patient Profile...");

            try
            {
                await SavePatientProfile(fullName, dob, gender, contactNumber, email, diagnosis, therapyStartDate, therapistName, causeOfImpairment, severityLevel, caregiverFullName, caregiverRelation, caregiverOccupation, caregiverContactNumber, caregiverEmail);
                Debug.Log("Patient Profile Created Successfully!");
                SceneManager.LoadScene("PatientHomePage");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error saving patient profile: " + ex.Message);
                errorText.text = "Failed to save profile. Please try again.";
            }
        }
    }

    async Task SavePatientProfile(string fullName, string dob, string gender, string contactNumber, string email,
     string diagnosis, string therapyStartDate, string therapistName, string causeOfImpairment, float severityLevel,
     string caregiverFullName, string caregiverRelation, string caregiverOccupation, string caregiverContactNumber, string caregiverEmail)
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        var profileData = new Dictionary<string, object>
        {
            { "fullName", fullName },
            { "dob", dob },
            { "gender", gender },
            { "contactNumber", contactNumber },
            { "email", email },
            { "diagnosis", diagnosis },
            { "therapyStartDate", therapyStartDate },
            { "therapistName", therapistName },
            { "causeOfImpairment", causeOfImpairment },
            { "severityLevel", severityLevel },
            { "caregiverFullName", caregiverFullName },
            { "caregiverRelation", caregiverRelation },
            { "caregiverOccupation", caregiverOccupation },
            { "caregiverContactNumber", caregiverContactNumber },
            { "caregiverEmail", caregiverEmail }
        };

        var data = new Dictionary<string, object>
        {
            { $"{playerId}_PatientDetails", profileData }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }


    bool IsValidProfile(string fullName, string dob, string gender, string contactNumber, string email, string diagnosis, string therapyStartDate, string therapistName, string causeOfImpairment, string caregiverFullName, string caregiverRelation, string caregiverOccupation, string caregiverContactNumber, string caregiverEmail)
    {
        if (!ValidateName(fullName)) return false;
        if (string.IsNullOrEmpty(dob)) return ShowError("Date of Birth cannot be empty.");
        if (genderDropdown.value == 0) return ShowError("Please select a gender.");
        if (!ValidateContactNumber(contactNumber)) return false;
        if (!ValidateEmail(email)) return false;
        if (string.IsNullOrEmpty(diagnosis)) return ShowError("Diagnosis cannot be empty.");
        if (string.IsNullOrEmpty(therapyStartDate)) return ShowError("Therapy Start Date cannot be empty.");
        if (string.IsNullOrEmpty(therapistName)) return ShowError("Therapist Name cannot be empty.");
        if (causeOfImpairmentDropdown.value == 0) return ShowError("Please select a valid cause of impairment.");
        if (!ValidateName(caregiverFullName)) return false;
        if (string.IsNullOrEmpty(caregiverRelation)) return ShowError("Relation to Patient cannot be empty.");
        if (string.IsNullOrEmpty(caregiverOccupation)) return ShowError("Caregiver Occupation cannot be empty.");
        if (!ValidateContactNumber(caregiverContactNumber)) return false;
        if (!ValidateEmail(caregiverEmail)) return false;

        errorText.text = "";
        return true;

    }
    bool ValidateName(string name)
    {
        if (!Regex.IsMatch(name, "^[a-zA-Z ]+$")) return ShowError("Invalid Name. Only letters and spaces allowed.");
        return true;
    }

    bool ValidateContactNumber(string number)
    {
        if (!Regex.IsMatch(number, @"^\d{10,15}$")) return ShowError("Invalid Contact Number. It should be 10-15 digits long.");
        return true;
    }

    bool ValidateEmail(string email)
    {
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return ShowError("Invalid Email Format.");
        return true;
    }

    bool ShowError(string message)
    {
        errorText.text = message;
        return false;
    }
}
