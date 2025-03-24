using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using TMPro;

public class UserNameUpdater : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text WelcomeText; // Assign this in the Inspector (e.g., "Welcome, Dobby!")

    async void Start()
    {
        Debug.Log("Initializing Unity Services...");
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Signing in anonymously...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("✅ Signed in successfully.");
        }

        // Fetch and display the user's full name
        await LoadUserName();
    }

    private async Task LoadUserName()
    {
        try
        {
            Debug.Log("🔄 Fetching user profile from Cloud Save...");

            var profileResponse = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "PatientProfile" });

            if (profileResponse.TryGetValue("PatientProfile", out var profileItem))
            {
                string profileJson = profileItem.Value.GetAsString();
                Debug.Log($"📊 Raw PatientProfile Data: {profileJson}");

                // Parse the JSON to extract fullName
                PatientProfile patientProfile = JsonUtility.FromJson<PatientProfile>(profileJson);

                if (patientProfile != null)
                {
                    // Display the welcome message with the user's full name
                    WelcomeText.text = $"Hey, {patientProfile.fullName}!";

                    Debug.Log($"✅ Welcome message updated successfully: {WelcomeText.text}");
                }
                else
                {
                    Debug.LogError("❌ Failed to parse PatientProfile.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No PatientProfile found in Cloud Save.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error while fetching user profile: {e.Message}");
        }
    }

    // Data structure for PatientProfile (only including fullName)
    [System.Serializable]
    public class PatientProfile
    {
        public string fullName; // The full name of the patient
    }
}
