using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using Newtonsoft.Json;
using System.Collections;

public class LevelMenu : MonoBehaviour
{
    public Button[] buttons;
    public GameObject levelButtons;
    private bool isInitialized = false;

    [Serializable]
    public class LevelData
    {
        public int unlockedLevel;
        public Dictionary<int, bool> completedLevels;

        public LevelData()
        {
            unlockedLevel = 1; // Start with level 1 unlocked
            completedLevels = new Dictionary<int, bool>();
        }
    }

    private LevelData levelData;
    private static Queue<Action> executionQueue = new Queue<Action>();

    private void Awake()
    {
        ButtonsToArray();
        DisableAllButtons();
    }

    private async void Start()
    {
        try
        {
            await InitializeServices();
            await LoadOrInitializeLevelData();
            UpdateButtonStates();
            isInitialized = true;
            Debug.Log("[LevelMenu] Initialization complete");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelMenu] Start error: {e.Message}");
        }
    }

    private void OnEnable()
    {
        if (isInitialized)
        {
            StartCoroutine(RefreshLevelData());
        }
    }

    private IEnumerator RefreshLevelData()
    {
        var loadTask = LoadOrInitializeLevelData();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception == null)
        {
            UpdateButtonStates();
        }
        else
        {
            Debug.LogError($"[LevelMenu] Refresh error: {loadTask.Exception.Message}");
        }
    }

    private async Task InitializeServices()
    {
        try
        {
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("[LevelMenu] Player is not signed in! Redirecting to Sign-In page...");
                SceneManager.LoadScene("SignInPage"); // Ensure you have a sign-in scene
                return;
            }

            Debug.Log($"[LevelMenu] Signed in as Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelMenu] Service initialization failed: {e.Message}");
            SceneManager.LoadScene("SignInPage"); // Redirect if there's an issue
        }
    }


    private async Task LoadOrInitializeLevelData()
    {
        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { "LevelData" });

            if (result.TryGetValue("LevelData", out var savedData))
            {
                string json = JsonConvert.SerializeObject(savedData.Value);
                levelData = JsonConvert.DeserializeObject<LevelData>(json);
                Debug.Log($"[LevelMenu] Loaded level data. Unlocked up to level: {levelData.unlockedLevel}");
            }
            else
            {
                levelData = new LevelData();
                await SaveLevelData();
                Debug.Log("[LevelMenu] Initialized new level data with level 1 unlocked");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelMenu] Error loading level data: {e.Message}");
            levelData = new LevelData();
        }
    }

    private async Task SaveLevelData()
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["LevelData"] = levelData
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"[LevelMenu] Saved level data. Unlocked up to level: {levelData.unlockedLevel}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelMenu] Error saving level data: {e.Message}");
        }
    }

    private void UpdateButtonStates()
    {
        if (levelData == null)
        {
            Debug.LogError("[LevelMenu] Level data is null during button update");
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int levelNumber = i + 1;
            bool isUnlocked = levelNumber <= levelData.unlockedLevel;

            if (buttons[i] != null)
            {
                buttons[i].interactable = isUnlocked;

                // Visual feedback for completed levels
                if (levelData.completedLevels.TryGetValue(levelNumber, out bool isCompleted) && isCompleted)
                {
                    var buttonImage = buttons[i].GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = Color.green;
                    }
                }
            }
            else
            {
                Debug.LogError($"[LevelMenu] Button at index {i} is null");
            }
        }
    }

    public void OpenLevel(int levelNumber)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[LevelMenu] Cannot open level before initialization");
            return;
        }

        if (levelNumber > levelData.unlockedLevel)
        {
            Debug.LogWarning($"[LevelMenu] Level {levelNumber} is not yet unlocked");
            return;
        }

        string levelName = "HC - Level " + levelNumber;  // Updated scene name format
        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogError($"[LevelMenu] Scene {levelName} not found!");
        }
    }


    private void DisableAllButtons()
    {
        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    private void ButtonsToArray()
    {
        if (levelButtons == null)
        {
            Debug.LogError("[LevelMenu] levelButtons GameObject is not assigned");
            return;
        }

        int childCount = levelButtons.transform.childCount;
        buttons = new Button[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = levelButtons.transform.GetChild(i);
            if (child != null)
            {
                buttons[i] = child.GetComponent<Button>();
                if (buttons[i] == null)
                {
                    Debug.LogError($"[LevelMenu] Button component not found on child {i}");
                }
            }
            else
            {
                Debug.LogError($"[LevelMenu] Child {i} is null");
            }
        }
    }

    public static async Task UnlockNextLevel(int completedLevelNumber)
    {
        try
        {
            Debug.Log($"[LevelMenu] Starting unlock process for level {completedLevelNumber}");

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("[LevelMenu] Not signed in to Authentication Service!");
                return;
            }

            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { "LevelData" });

            LevelData levelData;
            if (result.TryGetValue("LevelData", out var savedData))
            {
                string json = JsonConvert.SerializeObject(savedData.Value);
                levelData = JsonConvert.DeserializeObject<LevelData>(json);
                Debug.Log($"[LevelMenu] Current unlocked level: {levelData.unlockedLevel}");
            }
            else
            {
                levelData = new LevelData();
                Debug.Log("[LevelMenu] No existing data found, creating new");
            }

            // Mark current level as completed
            levelData.completedLevels[completedLevelNumber] = true;

            // Unlock next level
            if (completedLevelNumber >= levelData.unlockedLevel)
            {
                levelData.unlockedLevel = completedLevelNumber + 1;
                Debug.Log($"[LevelMenu] Unlocking next level. New unlocked level: {levelData.unlockedLevel}");
            }

            var dataToSave = new Dictionary<string, object>
            {
                ["LevelData"] = levelData
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log($"[LevelMenu] Successfully saved level data. Unlocked level: {levelData.unlockedLevel}");

            // Verify save
            var verificationResult = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { "LevelData" });
            if (verificationResult.TryGetValue("LevelData", out var verifiedData))
            {
                string verifiedJson = JsonConvert.SerializeObject(verifiedData.Value);
                var verifiedLevelData = JsonConvert.DeserializeObject<LevelData>(verifiedJson);
                Debug.Log($"[LevelMenu] Verification - Current unlocked level: {verifiedLevelData.unlockedLevel}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelMenu] Error in UnlockNextLevel: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }
}