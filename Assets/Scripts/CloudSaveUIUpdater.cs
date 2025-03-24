using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using TMPro;

public class CloudSaveUIUpdater : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text WelcomeText;       // Displays the user's full name
    public TMP_Text GameNameText;      // Displays the game name
    public TMP_Text UnlockedLevelText; // Displays the unlocked level

    private const string GameName = "HillClimber"; // Change this to your actual game name

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

        // Fetch and update UI with user name and game progress
        await LoadUserName();
        await LoadGameAndLevelData();
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

                PatientProfile patientProfile = JsonUtility.FromJson<PatientProfile>(profileJson);
                if (patientProfile != null)
                {
                    WelcomeText.text = $"Welcome, {patientProfile.fullName}!";
                    Debug.Log($"✅ Welcome message updated: {WelcomeText.text}");
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
            Debug.LogError($"❌ Error fetching user profile: {e.Message}");
        }
    }

    private async Task LoadGameAndLevelData()
    {
        try
        {
            Debug.Log("🔄 Fetching game and level data from Cloud Save...");

            var levelDataResponse = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "LevelData" });

            if (levelDataResponse.TryGetValue("LevelData", out var levelDataItem))
            {
                string levelDataJson = levelDataItem.Value.GetAsString();
                Debug.Log($"📊 Raw LevelData: {levelDataJson}");

                LevelData levelData = JsonUtility.FromJson<LevelData>(levelDataJson);
                if (levelData != null)
                {
                    GameNameText.text = GameName;
                    UnlockedLevelText.text = $"Level Unlocked: {levelData.unlockedLevel}";
                    Debug.Log("✅ Game Name and Unlocked Level updated.");
                }
                else
                {
                    Debug.LogError("❌ Failed to parse LevelData.");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No LevelData found in Cloud Save.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error fetching game and level data: {e.Message}");
        }
    }

    [System.Serializable]
    public class PatientProfile
    {
        public string fullName;
    }

    [System.Serializable]
    public class LevelData
    {
        public int unlockedLevel;
    }
}
