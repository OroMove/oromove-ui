using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class TherapistProfileCreationController : MonoBehaviour
{
    public TMP_InputField fullNameInput;
    public TMP_InputField designationInput;
    public TMP_InputField practiceLocationInput;
    public TMP_Dropdown specializationDropdown; // Changed to TMP_Dropdown
    public TMP_InputField experienceInput;
    public TMP_InputField contactNumberInput;
    public TMP_InputField emailInput;

    public Button backButton;
    public Button doneButton;
    public TextMeshProUGUI errorText; // To display error messages

    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);

        // Populate dropdown with sample specializations
        specializationDropdown.ClearOptions();
        specializationDropdown.AddOptions(new List<string>
        {
            "Select Specialization",
            "Speech Language Pathologist",
            "Audiologist",
            "Occupational Therapist",
            "Physical Therapist"
        });
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("SignUpPage");
    }

    async void OnDoneButtonClicked()
    {
        string fullName = fullNameInput.text.Trim();
        string designation = designationInput.text.Trim();
        string practiceLocation = practiceLocationInput.text.Trim();
        string specialization = specializationDropdown.options[specializationDropdown.value].text; // Get selected specialization
        string experience = experienceInput.text.Trim();
        string contactNumber = contactNumberInput.text.Trim();
        string email = emailInput.text.Trim();

        if (IsValidProfile(fullName, designation, practiceLocation, specialization, experience, contactNumber, email))
        {
            Debug.Log("Saving Therapist Profile...");

            try
            {
                await SaveTherapistProfile(fullName, designation, practiceLocation, specialization, experience, contactNumber, email);
                Debug.Log("Therapist Profile Created Successfully!");
                SceneManager.LoadScene("TherapistHomePage");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error saving therapist profile: " + ex.Message);
                errorText.text = "Failed to save profile. Please try again.";
            }
        }
    }

    async Task SaveTherapistProfile(string fullName, string designation, string practiceLocation, string specialization, string experience, string contactNumber, string email)
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        var data = new Dictionary<string, object>
        {
            { $"{playerId}_fullName", fullName },
            { $"{playerId}_designation", designation },
            { $"{playerId}_practiceLocation", practiceLocation },
            { $"{playerId}_specialization", specialization },
            { $"{playerId}_experience", experience },
            { $"{playerId}_contactNumber", contactNumber },
            { $"{playerId}_email", email }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    bool IsValidProfile(string fullName, string designation, string practiceLocation, string specialization, string experience, string contactNumber, string email)
    {
        if (string.IsNullOrEmpty(fullName) || !Regex.IsMatch(fullName, "^[a-zA-Z ]+$"))
        {
            errorText.text = "Invalid Name. Only letters and spaces allowed.";
            return false;
        }

        if (string.IsNullOrEmpty(designation))
        {
            errorText.text = "Designation cannot be empty.";
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

        errorText.text = "";
        return true;
    }
}
