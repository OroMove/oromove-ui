using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditProfileController : MonoBehaviour
{
    // Panels
    public GameObject personalPanel,
        medicalPanel,
        caretakerPanel;
    public Button personalButton,
        medicalButton,
        caretakerButton;

    // PERSONAL INFO
    public TextMeshProUGUI fullNameText,
        dobText,
        contactNumberText,
        emailText,
        genderText;
    public TMP_InputField fullNameInput,
        dobInput,
        contactNumberInput,
        emailInput;
    public TMP_Dropdown genderDropdown;

    // MEDICAL INFO
    public TextMeshProUGUI diagnosisText,
        therapyStartDateText,
        therapistNameText,
        causeOfImpairmentText,
        severityText;
    public TMP_InputField diagnosisInput,
        therapyStartDateInput,
        therapistNameInput;
    public TMP_Dropdown causeOfImpairmentDropdown;
    public Slider severityLevelSlider;
    public TextMeshProUGUI severityValueText;

    // CAREGIVER INFO
    public TextMeshProUGUI caregiverNameText,
        caregiverEmailText,
        caregiverContactText,
        caregiverOccupationText,
        caregiverRelationText;
    public TMP_InputField caregiverNameInput,
        caregiverEmailInput,
        caregiverContactInput,
        caregiverOccupationInput,
        caregiverRelationInput;

    //EDIT BUTTONS FOR EACH FIELD
    public Button editFullNameButton,
        editDobButton,
        editContactButton,
        editEmailButton,
        editGenderButton;
    public Button editDiagnosisButton,
        editTherapyStartDateButton,
        editTherapistNameButton,
        editCauseOfImpairmentButton,
        editSeverityButton;
    public Button editCaregiverNameButton,
        editCaregiverEmailButton,
        editCaregiverContactButton,
        editCaregiverOccupationButton,
        editCaregiverRelationButton;

    public Button saveButton,
        backButton;

    private Dictionary<string, object> patientProfile;

    void Start()
    {
        InitializeUnityServices();

        saveButton.onClick.AddListener(OnSaveButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        personalButton.onClick.AddListener(() => ShowPanel(personalPanel));
        medicalButton.onClick.AddListener(() => ShowPanel(medicalPanel));
        caretakerButton.onClick.AddListener(() => ShowPanel(caretakerPanel));

        //Hide inout fields initially
        fullNameInput.gameObject.SetActive(false);
        dobInput.gameObject.SetActive(false);
        contactNumberInput.gameObject.SetActive(false);
        emailInput.gameObject.SetActive(false);
        genderDropdown.gameObject.SetActive(false);

        diagnosisInput.gameObject.SetActive(false);
        therapyStartDateInput.gameObject.SetActive(false);
        therapistNameInput.gameObject.SetActive(false);
        causeOfImpairmentDropdown.gameObject.SetActive(false);
        severityLevelSlider.gameObject.SetActive(false);

        caregiverNameInput.gameObject.SetActive(false);
        caregiverEmailInput.gameObject.SetActive(false);
        caregiverContactInput.gameObject.SetActive(false);
        caregiverOccupationInput.gameObject.SetActive(false);
        caregiverRelationInput.gameObject.SetActive(false);

        // Assign Edit Button Listeners
        editFullNameButton.onClick.AddListener(() => ToggleEditMode(fullNameText, fullNameInput));
        editDobButton.onClick.AddListener(() => ToggleEditMode(dobText, dobInput));
        editContactButton.onClick.AddListener(
            () => ToggleEditMode(contactNumberText, contactNumberInput)
        );
        editEmailButton.onClick.AddListener(() => ToggleEditMode(emailText, emailInput));
        editGenderButton.onClick.AddListener(() => ToggleEditMode(genderText, genderDropdown));

        editDiagnosisButton.onClick.AddListener(
            () => ToggleEditMode(diagnosisText, diagnosisInput)
        );
        editTherapyStartDateButton.onClick.AddListener(
            () => ToggleEditMode(therapyStartDateText, therapyStartDateInput)
        );
        editTherapistNameButton.onClick.AddListener(
            () => ToggleEditMode(therapistNameText, therapistNameInput)
        );
        editCauseOfImpairmentButton.onClick.AddListener(
            () => ToggleEditMode(causeOfImpairmentText, causeOfImpairmentDropdown)
        );
        editSeverityButton.onClick.AddListener(
            () => ToggleEditMode(severityText, severityLevelSlider)
        );

        editCaregiverNameButton.onClick.AddListener(
            () => ToggleEditMode(caregiverNameText, caregiverNameInput)
        );
        editCaregiverEmailButton.onClick.AddListener(
            () => ToggleEditMode(caregiverEmailText, caregiverEmailInput)
        );
        editCaregiverContactButton.onClick.AddListener(
            () => ToggleEditMode(caregiverContactText, caregiverContactInput)
        );
        editCaregiverOccupationButton.onClick.AddListener(
            () => ToggleEditMode(caregiverOccupationText, caregiverOccupationInput)
        );
        editCaregiverRelationButton.onClick.AddListener(
            () => ToggleEditMode(caregiverRelationText, caregiverRelationInput)
        );

        LoadPatientProfileData();
        ShowPanel(personalPanel); // Default panel to show
    }

    async void InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously.");
            }

            await LoadPatientProfileData(); // Load data after initialization
            ShowPanel(personalPanel); // Default panel to show
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to initialize Unity Services: " + ex.Message);
        }
    }

    void ShowPanel(GameObject panelToShow)
    {
        personalPanel.SetActive(panelToShow == personalPanel);
        medicalPanel.SetActive(panelToShow == medicalPanel);
        caretakerPanel.SetActive(panelToShow == caretakerPanel);
    }

    async Task LoadPatientProfileData()
    {
        var savedData = await CloudSaveService.Instance.Data.LoadAsync(
            new HashSet<string> { "PatientProfile" }
        );

        if (savedData.TryGetValue("PatientProfile", out var profileItem))
        {
            try
            {
                // Convert JsonObject to JSON string
                string jsonString = profileItem.ToString(); // Ensure profileItem is a string

                // Deserialize JSON into Dictionary
                Dictionary<string, object> profileData = JsonConvert.DeserializeObject<
                    Dictionary<string, object>
                >(jsonString);
                patientProfile = new Dictionary<string, object>(profileData);

                // Example: Access data and check if the keys exist
                Debug.Log($"Patient Name: {GetValueOrDefault(profileData, "fullName")}");
                Debug.Log($"Patient DoB: {GetValueOrDefault(profileData, "dob")}");

                fullNameText.text = GetValueOrDefault(profileData, "fullName");
                dobText.text = GetValueOrDefault(profileData, "dob");
                contactNumberText.text = GetValueOrDefault(profileData, "contactNumber");
                emailText.text = GetValueOrDefault(profileData, "email");
                genderText.text = GetValueOrDefault(profileData, "gender");

                diagnosisText.text = GetValueOrDefault(profileData, "diagnosis");
                therapyStartDateText.text = GetValueOrDefault(profileData, "therapyStartDate");
                therapistNameText.text = GetValueOrDefault(profileData, "therapistName");
                causeOfImpairmentText.text = GetValueOrDefault(profileData, "causeOfImpairment");
                severityText.text = GetValueOrDefault(profileData, "severityLevel").ToString();

                caregiverNameText.text = GetValueOrDefault(profileData, "caregiverFullName");
                caregiverEmailText.text = GetValueOrDefault(profileData, "caregiverEmail");
                caregiverContactText.text = GetValueOrDefault(
                    profileData,
                    "caregiverContactNumber"
                );
                caregiverOccupationText.text = GetValueOrDefault(
                    profileData,
                    "caregiverOccupation"
                );
                caregiverRelationText.text = GetValueOrDefault(profileData, "caregiverRelation");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing PatientProfile: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("PatientProfile data not found.");
        }
    }

    private string GetValueOrDefault(Dictionary<string, object> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out var value) ? value.ToString() : "N/A";
    }

    void UpdateSeverityText(float value)
    {
        string[] severityLabels = { "Mild", "Moderate", "Severe" };
        int index = Mathf.Clamp((int)value, 0, severityLabels.Length - 1);
        severityValueText.text = severityLabels[index];
    }

    async void OnSaveButtonClicked()
    {
        // Check for changes and update only modified fields
        if (!string.IsNullOrEmpty(fullNameInput.text))
            patientProfile["fullName"] = fullNameInput.text;
        if (!string.IsNullOrEmpty(dobInput.text))
            patientProfile["dob"] = dobInput.text;
        if (!string.IsNullOrEmpty(contactNumberInput.text))
            patientProfile["contactNumber"] = contactNumberInput.text;
        if (!string.IsNullOrEmpty(emailInput.text))
            patientProfile["email"] = emailInput.text;
        if (genderDropdown.value > 0)
            patientProfile["gender"] = genderDropdown.options[genderDropdown.value].text;

        if (!string.IsNullOrEmpty(diagnosisInput.text))
            patientProfile["diagnosis"] = diagnosisInput.text;
        if (!string.IsNullOrEmpty(therapyStartDateInput.text))
            patientProfile["therapyStartDate"] = therapyStartDateInput.text;
        if (!string.IsNullOrEmpty(therapistNameInput.text))
            patientProfile["therapistName"] = therapistNameInput.text;
        if (causeOfImpairmentDropdown.value > 0)
            patientProfile["causeOfImpairment"] = causeOfImpairmentDropdown
                .options[causeOfImpairmentDropdown.value]
                .text;
        patientProfile["severityLevel"] = severityLevelSlider.value;

        if (!string.IsNullOrEmpty(caregiverNameInput.text))
            patientProfile["caregiverFullName"] = caregiverNameInput.text;
        if (!string.IsNullOrEmpty(caregiverEmailInput.text))
            patientProfile["caregiverEmail"] = caregiverEmailInput.text;
        if (!string.IsNullOrEmpty(caregiverContactInput.text))
            patientProfile["caregiverContactNumber"] = caregiverContactInput.text;
        if (!string.IsNullOrEmpty(caregiverOccupationInput.text))
            patientProfile["caregiverOccupation"] = caregiverOccupationInput.text;
        if (!string.IsNullOrEmpty(caregiverRelationInput.text))
            patientProfile["caregiverRelation"] = caregiverRelationInput.text;

        // Save the updated data
        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(
                new Dictionary<string, object> { { "PatientProfile", patientProfile } }
            );
            Debug.Log("Profile data saved successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error saving profile data: " + ex.Message);
        }
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("PatientSettingsPage");
    }

    // Toggles between edit mode and view mode
    void ToggleEditMode(TextMeshProUGUI textElement, TMP_InputField inputField)
    {
        if (inputField.gameObject.activeSelf)
        {
            // Save the edited value
            textElement.text = inputField.text;
            inputField.gameObject.SetActive(false);
            textElement.gameObject.SetActive(true);
        }
        else
        {
            // Switch to edit mode
            inputField.text = textElement.text;
            inputField.gameObject.SetActive(true);
            textElement.gameObject.SetActive(false);
        }
    }

    void ToggleEditMode(TextMeshProUGUI textElement, TMP_Dropdown dropdown)
    {
        if (dropdown.gameObject.activeSelf)
        {
            // Save the edited value
            textElement.text = dropdown.options[dropdown.value].text;
            dropdown.gameObject.SetActive(false);
            textElement.gameObject.SetActive(true);
        }
        else
        {
            // Switch to edit mode
            dropdown.value = dropdown.options.FindIndex(option => option.text == textElement.text);
            dropdown.gameObject.SetActive(true);
            textElement.gameObject.SetActive(false);
        }
    }

    void ToggleEditMode(TextMeshProUGUI textElement, Slider slider)
    {
        if (slider.gameObject.activeSelf)
        {
            // Save the edited value
            textElement.text = slider.value.ToString("0.0");
            slider.gameObject.SetActive(false);
            textElement.gameObject.SetActive(true);
        }
        else
        {
            // Switch to edit mode
            slider.value = float.Parse(textElement.text);
            slider.gameObject.SetActive(true);
            textElement.gameObject.SetActive(false);
        }

        // Add listener and update text in both modes
        severityLevelSlider.onValueChanged.RemoveListener(UpdateSeverityText); // Remove previous listeners to prevent duplicates
        severityLevelSlider.onValueChanged.AddListener(UpdateSeverityText); // Add the listener again
        UpdateSeverityText(severityLevelSlider.value);
    }
}
