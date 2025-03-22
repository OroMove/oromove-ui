using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using TMPro;

public class Graph : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown levelDropdown;    // Dropdown for selecting level
    public GameObject graphContainer;     // The container where the graph will be drawn
    public GameObject linePrefab;         // Prefab for drawing the line (Must be assigned in Inspector)
    public Button LoadDataButton;         // Button to fetch and display data
    public TMP_Text NoDataText; // For TextMeshPro components
                                // UI Text to display "Please play this level" message

    // UI for Game Name and Unlocked Level
    public TMP_Text GameNameText;    // Text to display Game Name
    public TMP_Text UnlockedLevelText; // Text to display Unlocked Level

    private Dictionary<int, List<float>> levelMaxMouthOpeningData = new Dictionary<int, List<float>>();

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

        // Ensure the dropdown is properly assigned
        if (levelDropdown == null)
        {
            levelDropdown = GameObject.Find("LevelDropdown").GetComponent<TMP_Dropdown>();
        }

        if (levelDropdown != null)
        {
            Debug.Log("✅ TMP Dropdown found and assigned!");
        }
        else
        {
            Debug.LogError("❌ TMP Dropdown not found! Check the GameObject name.");
        }

        // Populate the dropdown with level options
        PopulateDropdown();

        // Fetch progress data from Cloud Save
        await LoadProgressData();

        // Attach event listener to LoadDataButton
        LoadDataButton.onClick.AddListener(OnLoadDataButtonClicked);

        // Fetch and display Game Name and Unlocked Level
        await LoadGameAndLevelData();

        // Set default graph (Level 1)
        DrawGraph(1);
    }

    private void PopulateDropdown()
    {
        levelDropdown.ClearOptions();   // Clear any existing options
        List<string> levels = new List<string>();

        for (int i = 1; i <= 5; i++)  // Assuming 5 levels
        {
            levels.Add($"Level {i}");
        }

        levelDropdown.AddOptions(levels);  // Add level options to the dropdown
    }

    private async Task LoadProgressData()
    {
        try
        {
            Debug.Log("🔄 Fetching progress data from Cloud Save...");

            for (int level = 1; level <= 5; level++) // Assuming 5 levels
            {
                List<float> maxMouthOpenings = new List<float>();

                for (int attempt = 1; attempt <= 5; attempt++) // Assuming 5 attempts per level
                {
                    string attemptKey = $"Level_{level}_Attempt_{attempt}";
                    var attemptResponse = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { attemptKey });

                    if (attemptResponse.TryGetValue(attemptKey, out var attemptItem))
                    {
                        var attemptJson = attemptItem.Value.GetAsString();
                        Debug.Log($"📊 Raw Data for {attemptKey}: " + attemptJson);

                        LevelAttemptData attemptData = JsonUtility.FromJson<LevelAttemptData>(attemptJson);

                        if (attemptData != null)
                        {
                            maxMouthOpenings.Add(attemptData.MaxMouthOpening);
                            Debug.Log($"✅ Level {level} Attempt {attempt}: Max Mouth Opening = {attemptData.MaxMouthOpening}");
                        }
                        else
                        {
                            Debug.LogError($"❌ Failed to parse data for {attemptKey}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ No data found for {attemptKey}");
                    }
                }

                if (maxMouthOpenings.Count > 0)
                {
                    levelMaxMouthOpeningData[level] = maxMouthOpenings;
                    Debug.Log($"✅ Data for Level {level} saved successfully.");
                }
                else
                {
                    Debug.LogWarning($"⚠️ No valid data found for Level {level}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error while fetching data: {e.Message}");
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

                // Parse the LevelData JSON
                LevelData levelData = JsonUtility.FromJson<LevelData>(levelDataJson);

                if (levelData != null)
                {
                    // Display the Game Name
                    GameNameText.text = "HillClimber";  // Put your game name here
                    // Display the Unlocked Level
                    UnlockedLevelText.text = $"Unlocked Level: {levelData.unlockedLevel}";

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

    private void OnLoadDataButtonClicked()
    {
        int selectedLevel = levelDropdown.value + 1;  // Convert dropdown index to level number (1-based)
        DrawGraph(selectedLevel);
    }

    private void DrawGraph(int level)
    {
        // Clear previous graph elements
        foreach (Transform child in graphContainer.transform)
        {
            Destroy(child.gameObject);
        }

        if (!levelMaxMouthOpeningData.ContainsKey(level))
        {
            Debug.LogWarning($" No data found for Level {level}. Prompting user to play.");
            NoDataText.text = " Keep going! You haven’t unlocked this level yet, but you’re close!!";
            NoDataText.gameObject.SetActive(true);
            return;
        }

        NoDataText.gameObject.SetActive(false);  // Hide the "No Data" message
        List<float> data = levelMaxMouthOpeningData[level];

        // Validate Prefab Assignment
        if (linePrefab == null)
        {
            Debug.LogError("❌ Line Prefab is not assigned! Assign it in the Inspector.");
            return;
        }

        float graphWidth = graphContainer.GetComponent<RectTransform>().rect.width;
        float stepSize = graphWidth / (data.Count + 1); // Ensure spacing between bars
        float maxHeight = 300f;  // Adjust based on UI scaling

        for (int i = 0; i < data.Count; i++)
        {
            float xPosition = (i + 1) * stepSize;
            float yPosition = Mathf.Clamp(data[i] * 380f, 20f, maxHeight); // Scale mouth opening values

            GameObject line = Instantiate(linePrefab, graphContainer.transform);
            RectTransform lineRectTransform = line.GetComponent<RectTransform>();
            lineRectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            lineRectTransform.sizeDelta = new Vector2(37f, 45f);  // Thin vertical bars
        }

        Debug.Log($"✅ Graph for Level {level} drawn successfully.");
    }

    [System.Serializable]
    public class LevelAttemptData
    {
        public float MaxMouthOpening;
    }

    [System.Serializable]
    public class LevelData
    {
        public Dictionary<string, bool> completedLevels;
        public int unlockedLevel;
    }
}
