using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using TMPro;

public class GameInfoUpdater : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text GameNameText;    // Text component to display the game name
    public TMP_Text UnlockedLevelText; // Text component to display the unlocked level

    // The game name to display
    private const string GameName = "HillClimber"; // You can change this to your actual game name

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

        // Fetch and display the game name and unlocked level
        await LoadGameAndLevelData();
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

                // Parse the LevelData JSON
                LevelData levelData = JsonUtility.FromJson<LevelData>(levelDataJson);

                if (levelData != null)
                {
                    // Display the Game Name
                    GameNameText.text = GameName;  // Set your game name here
                    // Display the Unlocked Level
                    UnlockedLevelText.text = $"Level Unlocked: {levelData.unlockedLevel}";

                    Debug.Log($"✅ Game Name and Unlocked Level displayed successfully.");
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
            Debug.LogError($"❌ Error while fetching game and level data: {e.Message}");
        }
    }

    // Data structure for LevelData (assumed to be stored in Cloud Save)
    [System.Serializable]
    public class LevelData
    {
        public int unlockedLevel; // The level that is unlocked
    }
}
