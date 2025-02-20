using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class FinishPoint : MonoBehaviour
{
    public GameObject levelCompletePanel;
    public Button nextLevelButton;
    private bool levelCompletionProcessed = false;

    private void Start()
    {
        levelCompletePanel.SetActive(false);
        nextLevelButton.onClick.AddListener(LoadNextLevel);
    }

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !levelCompletionProcessed)
        {
            levelCompletionProcessed = true; // Prevent multiple triggers
            Debug.Log("[FinishPoint] Player reached finish line!");

            string currentSceneName = SceneManager.GetActiveScene().name;
            if (int.TryParse(currentSceneName.Replace("HC - Level ", ""), out int currentLevelNumber)) // Updated format
            {
                Debug.Log($"[FinishPoint] Current level number: {currentLevelNumber}");

                // Show completion panel
                levelCompletePanel.SetActive(true);

                // Directly handle level unlocking here for more control
                await UnlockNextLevelDirectly(currentLevelNumber);
            }
            else
            {
                Debug.LogError("[FinishPoint] Could not parse level number from scene name: " + currentSceneName);
            }
        }
    }

    private async Task UnlockNextLevelDirectly(int currentLevelNumber)
    {
        try
        {
            // Verify authentication
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("[FinishPoint] Not signed in to Authentication Service!");
                return;
            }

            Debug.Log($"[FinishPoint] Starting level unlock process for level {currentLevelNumber}");

            // Load existing data
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { "LevelData" });

            LevelMenu.LevelData levelData;
            if (result.TryGetValue("LevelData", out var savedData))
            {
                string json = JsonConvert.SerializeObject(savedData.Value);
                levelData = JsonConvert.DeserializeObject<LevelMenu.LevelData>(json);
                Debug.Log($"[FinishPoint] Loaded existing level data. Current unlocked level: {levelData.unlockedLevel}");
            }
            else
            {
                levelData = new LevelMenu.LevelData();
                Debug.Log("[FinishPoint] No existing level data found, created new data");
            }

            // Mark current level as completed
            levelData.completedLevels[currentLevelNumber] = true;

            // Unlock next level
            int nextLevelNumber = currentLevelNumber + 1;
            if (levelData.unlockedLevel <= nextLevelNumber)
            {
                levelData.unlockedLevel = nextLevelNumber;
                Debug.Log($"[FinishPoint] Unlocking next level. New unlocked level: {levelData.unlockedLevel}");
            }

            // Save updated data
            var dataToSave = new Dictionary<string, object>
            {
                ["LevelData"] = levelData
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log($"[FinishPoint] Successfully saved level data. Unlocked level: {levelData.unlockedLevel}");

            // Verify save
            var verificationResult = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { "LevelData" });
            if (verificationResult.TryGetValue("LevelData", out var verifiedData))
            {
                string verifiedJson = JsonConvert.SerializeObject(verifiedData.Value);
                var verifiedLevelData = JsonConvert.DeserializeObject<LevelMenu.LevelData>(verifiedJson);
                Debug.Log($"[FinishPoint] Verification - Current unlocked level: {verifiedLevelData.unlockedLevel}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FinishPoint] Error in UnlockNextLevelDirectly: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    private void LoadNextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (int.TryParse(currentSceneName.Replace("HC - Level ", ""), out int currentLevelNumber)) // Updated format
        {
            string nextLevelName = $"HC - Level {currentLevelNumber + 1}"; // Updated format
            Debug.Log($"[FinishPoint] Attempting to load next level: {nextLevelName}");

            if (Application.CanStreamedLevelBeLoaded(nextLevelName))
            {
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                Debug.Log("[FinishPoint] No more levels available!");
            }
        }
    }
}
