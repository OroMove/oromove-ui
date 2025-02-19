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
    public TMP_InputField therapyNameInput;
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

    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);

        // Populate gender dropdown
        genderDropdown.ClearOptions();
        genderDropdown.AddOptions(new List<string> { "Select Gender", "Male", "Female", "Other" });

        severityValueText.text = severityLevelSlider.value.ToString("0.0");
        severityLevelSlider.onValueChanged.AddListener(UpdateSeverityText);
    }

    void UpdateSeverityText(float value)
    {
        severityValueText.text = value.ToString("0.0");
    }

    void OnBackButtonClicked()
    {
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
        string therapyName = therapyNameInput.text.Trim();
        float severityLevel = severityLevelSlider.value;

        string caregiverFullName = caregiverFullNameInput.text.Trim();
        string caregiverRelation = caregiverRelationInput.text.Trim();
        string caregiverOccupation = caregiverOccupationInput.text.Trim();
        string caregiverContactNumber = caregiverContactNumberInput.text.Trim();
        string caregiverEmail = caregiverEmailInput.text.Trim();

        if (IsValidProfile(fullName, dob, gender, contactNumber, email, diagnosis, therapyStartDate, therapyName, caregiverFullName, caregiverRelation, caregiverOccupation, caregiverContactNumber, caregiverEmail))
        {
            Debug.Log("Saving Patient Profile...");

            try
            {
                await SavePatientProfile(fullName, dob, gender, contactNumber, email, diagnosis, therapyStartDate, therapyName, severityLevel, caregiverFullName, caregiverRelation, caregiverOccupation, caregiverContactNumber, caregiverEmail);
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

    async Task SavePatientProfile(string fullName, string dob, string gender, string contactNumber, string email, string diagnosis, string therapyStartDate, string therapyName, float severityLevel, string caregiverFullName, string caregiverRelation, string caregiverOccupation, string caregiverContactNumber, string caregiverEmail)
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        var data = new Dictionary<string, object>
        {
            { $"{playerId}_fullName", fullName },
            { $"{playerId}_dob", dob },
            { $"{playerId}_gender", gender },
            { $"{playerId}_contactNumber", contactNumber },
            { $"{playerId}_email", email },
            { $"{playerId}_diagnosis", diagnosis },
            { $"{playerId}_therapyStartDate", therapyStartDate },
            { $"{playerId}_therapyName", therapyName },
            { $"{playerId}_severityLevel", severityLevel },
            { $"{playerId}_caregiverFullName", caregiverFullName },
            { $"{playerId}_caregiverRelation", caregiverRelation },
            { $"{playerId}_caregiverOccupation", caregiverOccupation },
            { $"{playerId}_caregiverContactNumber", caregiverContactNumber },
            { $"{playerId}_caregiverEmail", caregiverEmail }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    bool IsValidProfile(string fullName, string dob, string gender, string contactNumber, string email, string diagnosis, string therapyStartDate, string therapyName, string caregiverFullName, string caregiverRelation, string caregiverOccupation, string caregiverContactNumber, string caregiverEmail)
    {
        if (string.IsNullOrEmpty(fullName) || !Regex.IsMatch(fullName, "^[a-zA-Z ]+$"))
        {
            errorText.text = "Invalid Name. Only letters and spaces allowed.";
            return false;
        }

        if (string.IsNullOrEmpty(dob))
        {
            errorText.text = "Date of Birth cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(gender) || gender == "Select Gender")
        {
            errorText.text = "Gender must be selected.";
            return false;
        }

        if (!Regex.IsMatch(contactNumber, @"^\d{10,15}$"))
        {
            errorText.text = "Invalid Contact Number. It should be 10-15 digits long.";
            return false;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            errorText.text = "Invalid Email Format.";
            return false;
        }

        if (string.IsNullOrEmpty(diagnosis))
        {
            errorText.text = "Diagnosis cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(therapyStartDate))
        {
            errorText.text = "Therapy Start Date cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(therapyName))
        {
            errorText.text = "Therapy Name cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(caregiverFullName) || !Regex.IsMatch(caregiverFullName, "^[a-zA-Z ]+$"))
        {
            errorText.text = "Invalid Caregiver Name. Only letters and spaces allowed.";
            return false;
        }

        if (string.IsNullOrEmpty(caregiverRelation))
        {
            errorText.text = "Relation to Patient cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(caregiverOccupation))
        {
            errorText.text = "Caregiver Occupation cannot be empty.";
            return false;
        }

        if (!Regex.IsMatch(caregiverContactNumber, @"^\d{10,15}$"))
        {
            errorText.text = "Invalid Caregiver Contact Number. It should be 10-15 digits long.";
            return false;
        }

        if (!Regex.IsMatch(caregiverEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            errorText.text = "Invalid Caregiver Email Format.";
            return false;
        }

        errorText.text = ""; 
        return true;
    }
}
