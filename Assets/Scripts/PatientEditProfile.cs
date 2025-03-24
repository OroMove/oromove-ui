using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class EditProfileController : MonoBehaviour
{
    // PERSONAL INFO
    public TextMeshProUGUI fullNameText, dobText, contactNumberText, emailText, genderText;
    public TMP_InputField fullNameInput, dobInput, contactNumberInput, emailInput;
    public TMP_Dropdown genderDropdown;

    // MEDICAL INFO
    public TextMeshProUGUI diagnosisText, therapyStartDateText, therapistNameText, causeOfImpairmentText, severityText;
    public TMP_InputField diagnosisInput, therapyStartDateInput, therapistNameInput;
    public TMP_Dropdown causeOfImpairmentDropdown;
    public Slider severityLevelSlider;

    // CAREGIVER INFO
    public TextMeshProUGUI caregiverNameText, caregiverEmailText, caregiverContactText, caregiverOccupationText, caregiverRelationText;
    public TMP_InputField caregiverNameInput, caregiverEmailInput, caregiverContactInput, caregiverOccupationInput, caregiverRelationInput;

    public Button saveButton;

    private Dictionary<string, object> patientProfile;

    void Start()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        LoadProfileData();
    }

    async void LoadProfileData()
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "PatientProfile" });

            if (data.TryGetValue("PatientProfile", out var profileItem))
            {
                patientProfile = profileItem.Value as Dictionary<string, object>;

                // Display existing data in TextMeshPro fields
                fullNameText.text = patientProfile["fullName"].ToString();
                dobText.text = patientProfile["dob"].ToString();
                contactNumberText.text = patientProfile["contactNumber"].ToString();
                emailText.text = patientProfile["email"].ToString();
                genderText.text = patientProfile["gender"].ToString();

                diagnosisText.text = patientProfile["diagnosis"].ToString();
                therapyStartDateText.text = patientProfile["therapyStartDate"].ToString();
                therapistNameText.text = patientProfile["therapistName"].ToString();
                causeOfImpairmentText.text = patientProfile["causeOfImpairment"].ToString();
                severityText.text = patientProfile["severityLevel"].ToString();

                caregiverNameText.text = patientProfile["caregiverFullName"].ToString();
                caregiverEmailText.text = patientProfile["caregiverEmail"].ToString();
                caregiverContactText.text = patientProfile["caregiverContactNumber"].ToString();
                caregiverOccupationText.text = patientProfile["caregiverOccupation"].ToString();
                caregiverRelationText.text = patientProfile["caregiverRelation"].ToString();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to load profile data: " + ex.Message);
        }
    }

    async void OnSaveButtonClicked()
    {
        // Check for changes and update only modified fields
        if (!string.IsNullOrEmpty(fullNameInput.text)) patientProfile["fullName"] = fullNameInput.text;
        if (!string.IsNullOrEmpty(dobInput.text)) patientProfile["dob"] = dobInput.text;
        if (!string.IsNullOrEmpty(contactNumberInput.text)) patientProfile["contactNumber"] = contactNumberInput.text;
        if (!string.IsNullOrEmpty(emailInput.text)) patientProfile["email"] = emailInput.text;
        if (genderDropdown.value > 0) patientProfile["gender"] = genderDropdown.options[genderDropdown.value].text;

        if (!string.IsNullOrEmpty(diagnosisInput.text)) patientProfile["diagnosis"] = diagnosisInput.text;
        if (!string.IsNullOrEmpty(therapyStartDateInput.text)) patientProfile["therapyStartDate"] = therapyStartDateInput.text;
        if (!string.IsNullOrEmpty(therapistNameInput.text)) patientProfile["therapistName"] = therapistNameInput.text;
        if (causeOfImpairmentDropdown.value > 0) patientProfile["causeOfImpairment"] = causeOfImpairmentDropdown.options[causeOfImpairmentDropdown.value].text;
        patientProfile["severityLevel"] = severityLevelSlider.value;

        if (!string.IsNullOrEmpty(caregiverNameInput.text)) patientProfile["caregiverFullName"] = caregiverNameInput.text;
        if (!string.IsNullOrEmpty(caregiverEmailInput.text)) patientProfile["caregiverEmail"] = caregiverEmailInput.text;
        if (!string.IsNullOrEmpty(caregiverContactInput.text)) patientProfile["caregiverContactNumber"] = caregiverContactInput.text;
        if (!string.IsNullOrEmpty(caregiverOccupationInput.text)) patientProfile["caregiverOccupation"] = caregiverOccupationInput.text;
        if (!string.IsNullOrEmpty(caregiverRelationInput.text)) patientProfile["caregiverRelation"] = caregiverRelationInput.text;

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { "PatientProfile", patientProfile } });
            Debug.Log("Profile updated successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save profile: " + ex.Message);
        }
    }
}
