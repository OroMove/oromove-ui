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

public class TherapistProfileCreationController : MonoBehaviour
{
    public TMP_InputField fullNameInput;
    public TMP_InputField practiceLocationInput;
    public TMP_Dropdown specializationDropdown;
    public TMP_Dropdown genderDropdown;
    public TMP_InputField experienceInput;
    public TMP_InputField contactNumberInput;
    public TMP_InputField emailInput;
    public TMP_InputField licenseNumberInput; // NEW FIELD

    public Button backButton;
    public Button doneButton;
    public TextMeshProUGUI errorText;

    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);

        // Populate specialization dropdown
        specializationDropdown.ClearOptions();
        specializationDropdown.AddOptions(
            new List<string>
            {
                "Select Specialization",
                "Speech Language Pathologist",
                "Audiologist",
                "Occupational Therapist",
                "Physical Therapist",
            }
        );

        // Populate gender dropdown
        genderDropdown.ClearOptions();
        genderDropdown.AddOptions(new List<string> { "Select Gender", "Male", "Female", "Other" });
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("SignUpPage");
    }

    async void OnDoneButtonClicked()
    {
        string fullName = fullNameInput.text.Trim();
        string practiceLocation = practiceLocationInput.text.Trim();
        string specialization = specializationDropdown.options[specializationDropdown.value].text;
        string gender = genderDropdown.options[genderDropdown.value].text;
        string contactNumber = contactNumberInput.text.Trim();
        string email = emailInput.text.Trim();
        string licenseNumber = licenseNumberInput.text.Trim(); // NEW

        if (!int.TryParse(experienceInput.text.Trim(), out int experienceYear))
        {
            errorText.text = "Experience must be a valid non-negative number.";
            return;
        }

        if (
            IsValidProfile(
                fullName,
                practiceLocation,
                specialization,
                gender,
                experienceInput.text.Trim(),
                contactNumber,
                email,
                licenseNumber
            )
        )
        {
            Debug.Log("Saving Therapist Profile...");

            try
            {
                await SaveTherapistProfile(
                    fullName,
                    practiceLocation,
                    specialization,
                    gender,
                    experienceYear,
                    contactNumber,
                    email,
                    licenseNumber
                );
                await TherapistSubmission(fullName, specialization, experienceYear);

                Debug.Log("Therapist Profile Created Successfully!");
                SceneManager.LoadScene("TherapistHomePage");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error saving therapist profile: " + ex.Message);
                errorText.text = "Failed to save profile. Please try again.";
            }
        }
    }

    async Task SaveTherapistProfile(
        string fullName,
        string practiceLocation,
        string specialization,
        string gender,
        int experience,
        string contactNumber,
        string email,
        string licenseNumber
    )
    {
        string playerId = AuthenticationService.Instance.PlayerId;
        var profileData = new Dictionary<string, object>
        {
            { "fullName", fullName },
            { "practiceLocation", practiceLocation },
            { "specialization", specialization },
            { "gender", gender },
            { "experience", experience },
            { "contactNumber", contactNumber },
            { "email", email },
            { "licenseNumber", licenseNumber }, // NEW
        };

        var data = new Dictionary<string, object> { { "TherapistProfile", profileData } };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    public async Task TherapistSubmission(string fullName, string specialization, int experience)
    {
        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(specialization))
        {
            Debug.LogError("Error: Name or Specialization is empty! Data will not be submitted.");
            return;
        }

        var options = new AddPlayerScoreOptions
        {
            Metadata = new Dictionary<string, string>
            {
                { "Name", fullName },
                { "Specialization", specialization },
            },
        };

        Debug.Log(
            $"Submitting to leaderboard: Name={fullName}, Specialization={specialization}, Experience={experience}"
        );

        try
        {
            var result = await LeaderboardsService.Instance.AddPlayerScoreAsync(
                "TherapistLeaderboard",
                experience,
                options
            );

            Debug.Log(
                $"Successfully submitted! Stored Metadata: {JsonConvert.SerializeObject(options.Metadata)}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to submit leaderboard entry: {ex.Message}");
        }
    }

    bool IsValidProfile(
        string fullName,
        string practiceLocation,
        string specialization,
        string gender,
        string experience,
        string contactNumber,
        string email,
        string licenseNumber
    )
    {
        if (string.IsNullOrEmpty(fullName) || !Regex.IsMatch(fullName, "^[a-zA-Z ]+$"))
        {
            errorText.text = "Invalid Name. Only letters and spaces allowed.";
            return false;
        }

        if (string.IsNullOrEmpty(practiceLocation))
        {
            errorText.text = "Practice Location cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(specialization) || specialization == "Select Specialization")
        {
            errorText.text = "Specialization must be selected.";
            return false;
        }

        if (string.IsNullOrEmpty(gender) || gender == "Select Gender")
        {
            errorText.text = "Gender must be selected.";
            return false;
        }

        if (!int.TryParse(experience, out int exp) || exp < 0)
        {
            errorText.text = "Experience must be a valid non-negative number.";
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

        if (
            string.IsNullOrEmpty(licenseNumber)
            || !Regex.IsMatch(licenseNumber, @"^[a-zA-Z0-9]{6,20}$")
        )
        {
            errorText.text = "Invalid License Number. It must be 6-20 alphanumeric characters.";
            return false;
        }

        errorText.text = "";
        return true;
    }
}
