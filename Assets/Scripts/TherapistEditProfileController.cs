using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;

public class TherapistEditProfileController : MonoBehaviour
{
    // Panels
    public GameObject personalPanel, therapistPanel;
    public Button personalButton, therapistButton;

    // PERSONAL INFO
    public TextMeshProUGUI fullNameText, contactNumberText, emailText, genderText;
    public TMP_InputField fullNameInput, contactNumberInput, emailInput;
    public TMP_Dropdown genderDropdown;

    // THERAPIST INFO
    public TextMeshProUGUI practiceLocationText, specializationText, experienceText, licenseNumberText;
    public TMP_InputField practiceLocationInput, experienceInput, licenseNumberInput;
    public TMP_Dropdown specializationDropdown;

    // EDIT BUTTONS
    public Button editFullNameButton, editContactButton, editEmailButton, editGenderButton;
    public Button editPracticeLocationButton, editSpecializationButton, editExperienceButton, editLicenseButton;

    public Button saveButton, backButton;
    private Dictionary<string, object> therapistProfile;

    async void Start()
    {
        InitializeUnityServices();

        saveButton.onClick.AddListener(OnSaveButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        personalButton.onClick.AddListener(() => ShowPanel(personalPanel));
        therapistButton.onClick.AddListener(() => ShowPanel(therapistPanel));

        // Hide input fields initially
        fullNameInput.gameObject.SetActive(false);
        contactNumberInput.gameObject.SetActive(false);
        emailInput.gameObject.SetActive(false);
        genderDropdown.gameObject.SetActive(false);
        practiceLocationInput.gameObject.SetActive(false);
        experienceInput.gameObject.SetActive(false);
        specializationDropdown.gameObject.SetActive(false);
        licenseNumberInput.gameObject.SetActive(false);

        // Assign Edit Button Listeners
        editFullNameButton.onClick.AddListener(() => ToggleEditMode(fullNameText, fullNameInput));
        editContactButton.onClick.AddListener(() => ToggleEditMode(contactNumberText, contactNumberInput));
        editEmailButton.onClick.AddListener(() => ToggleEditMode(emailText, emailInput));
        editGenderButton.onClick.AddListener(() => ToggleEditMode(genderText, genderDropdown));

        editPracticeLocationButton.onClick.AddListener(() => ToggleEditMode(practiceLocationText, practiceLocationInput));
        editSpecializationButton.onClick.AddListener(() => ToggleEditMode(specializationText, specializationDropdown));
        editExperienceButton.onClick.AddListener(() => ToggleEditMode(experienceText, experienceInput));
        editLicenseButton.onClick.AddListener(() => ToggleEditMode(licenseNumberText, licenseNumberInput));

        // Populate gender dropdown
        genderDropdown.ClearOptions();
        genderDropdown.AddOptions(new List<string> { "Select Gender", "Male", "Female", "Other" });

        await LoadTherapistProfileData();
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

            await LoadTherapistProfileData();
            ShowPanel(personalPanel); // Default panel to show
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize Unity Services: " + ex.Message);
        }
    }

    void ShowPanel(GameObject panelToShow)
    {
        personalPanel.SetActive(panelToShow == personalPanel);
        therapistPanel.SetActive(panelToShow == therapistPanel);
    }

    async Task LoadTherapistProfileData()
    {
        var keys = new HashSet<string> { "TherapistProfile" };

        // Load data asynchronously
        var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

        if (savedData.TryGetValue("TherapistProfile", out var profileItem))
        {
            try
            {
                // Get the serialized string from the Item's Value
                string jsonString = profileItem.Value.ToString();

                // Deserialize the JSON string into a dictionary
                Dictionary<string, object> profileData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                therapistProfile = new Dictionary<string, object>(profileData);

                // Set text values from loaded data
                fullNameText.text = GetValueOrDefault(profileData, "fullName");
                contactNumberText.text = GetValueOrDefault(profileData, "contactNumber");
                emailText.text = GetValueOrDefault(profileData, "email");
                genderText.text = GetValueOrDefault(profileData, "gender");
                practiceLocationText.text = GetValueOrDefault(profileData, "practiceLocation");
                specializationText.text = GetValueOrDefault(profileData, "specialization");
                experienceText.text = GetValueOrDefault(profileData, "experience");
                licenseNumberText.text = GetValueOrDefault(profileData, "licenseNumber");

            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing TherapistProfile: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("TherapistProfile data not found.");
        }
    }


    private string GetValueOrDefault(Dictionary<string, object> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out var value) ? value.ToString() : "N/A";
    }

    async void OnSaveButtonClicked()
    {
        if (!string.IsNullOrEmpty(fullNameInput.text)) therapistProfile["fullName"] = fullNameInput.text;
        if (!string.IsNullOrEmpty(contactNumberInput.text)) therapistProfile["contactNumber"] = contactNumberInput.text;
        if (!string.IsNullOrEmpty(emailInput.text)) therapistProfile["email"] = emailInput.text;

        if (genderDropdown.value > 0) therapistProfile["gender"] = genderDropdown.options[genderDropdown.value].text;
        if (!string.IsNullOrEmpty(practiceLocationInput.text)) therapistProfile["practiceLocation"] = practiceLocationInput.text;
        if (specializationDropdown.value > 0) therapistProfile["specialization"] = specializationDropdown.options[specializationDropdown.value].text;
        if (!string.IsNullOrEmpty(experienceInput.text)) therapistProfile["experience"] = experienceInput.text;
        if (!string.IsNullOrEmpty(licenseNumberInput.text)) therapistProfile["licenseNumber"] = licenseNumberInput.text;

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { "TherapistProfile", therapistProfile } });
            Debug.Log("Profile data saved successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving profile data: " + ex.Message);
        }
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("TherapistSettings");
    }

    // Toggles between edit mode and view mode
    void ToggleEditMode(TextMeshProUGUI textElement, TMP_InputField inputField)
    {
        if (inputField.gameObject.activeSelf)
        {
            textElement.text = inputField.text;
            inputField.gameObject.SetActive(false);
            textElement.gameObject.SetActive(true);
        }
        else
        {
            inputField.text = textElement.text;
            inputField.gameObject.SetActive(true);
            textElement.gameObject.SetActive(false);
        }
    }

    void ToggleEditMode(TextMeshProUGUI textElement, TMP_Dropdown dropdown)
    {
        if (dropdown.gameObject.activeSelf)
        {
            textElement.text = dropdown.options[dropdown.value].text;
            dropdown.gameObject.SetActive(false);
            textElement.gameObject.SetActive(true);
        }
        else
        {
            dropdown.value = dropdown.options.FindIndex(option => option.text == textElement.text);
            dropdown.gameObject.SetActive(true);
            textElement.gameObject.SetActive(false);
        }
    }
}
