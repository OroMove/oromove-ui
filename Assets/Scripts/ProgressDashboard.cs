using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ProgressDashboard : MonoBehaviour
{
    public TMP_Dropdown levelDropdown;
    public UnityEngine.UI.Button loadDataButton;
    public DD_DataDiagram dynamicChart;
    public DD_Lines dynamicLineChart;
    public UnityEngine.UI.Image loadingSpinner; // Loading spinner to indicate loading

    private async void Start()
    {
        await InitializeUnityServices();
        await AuthenticateUser();

        InitializeDropdown();
        loadDataButton.onClick.AddListener(OnLoadDataPressed);
    }

    private async Task InitializeUnityServices()
    {
        // Initializing Unity Services
        await Unity.Services.Core.UnityServices.InitializeAsync();
    }

    private async Task AuthenticateUser()
    {
        // Authenticating the user anonymously if not signed in
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void InitializeDropdown()
    {
        // Initialize level dropdown with options
        levelDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 1; i <= 5; i++)
        {
            options.Add($"Level {i}");
        }

        levelDropdown.AddOptions(options);
        levelDropdown.onValueChanged.AddListener(OnLevelChanged);
    }

    private async Task<Dictionary<int, List<float>>> LoadProgressData()
    {
        // This method will fetch the max mouth opening data for each level from Cloud Save
        Dictionary<int, List<float>> levelMaxMouthOpeningData = new Dictionary<int, List<float>>();

        for (int level = 1; level <= 5; level++)
        {
            List<float> maxMouthOpenings = new List<float>();

            for (int attempt = 1; attempt <= 5; attempt++)
            {
                string attemptKey = $"Level_{level}_Attempt_{attempt}";
                Debug.Log($"Fetching data for: {attemptKey}");

                // Fetching data from Cloud Save
                var attemptResponse = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { attemptKey });

                if (attemptResponse.TryGetValue(attemptKey, out var attemptItem))
                {
                    var attemptJson = attemptItem.Value.GetAsString();
                    LevelAttemptData attemptData = JsonConvert.DeserializeObject<LevelAttemptData>(attemptJson);
                    maxMouthOpenings.Add(attemptData.MaxMouthOpening);

                    Debug.Log($"✅ Data Found for {attemptKey}: {attemptData.MaxMouthOpening}");
                }
                else
                {
                    Debug.LogWarning($"❌ No data found for {attemptKey}");
                }
            }

            if (maxMouthOpenings.Count > 0)
            {
                levelMaxMouthOpeningData[level] = maxMouthOpenings;
            }
        }

        Debug.Log($"📊 Final Loaded Data: {JsonConvert.SerializeObject(levelMaxMouthOpeningData)}");
        return levelMaxMouthOpeningData;
    }

    private void UpdateGraph(Dictionary<int, List<float>> levelData)
    {
        if (dynamicChart == null)
        {
            Debug.LogError("❌ Dynamic Chart (DD_DataDiagram) is NOT assigned!");
            return;
        }

        DD_Lines[] lines = dynamicChart.GetComponentsInChildren<DD_Lines>();

        if (lines.Length == 0)
        {
            Debug.LogError("❌ No DD_Lines components found inside the graph!");
            return;
        }

        Debug.Log($"📈 Updating Graph with Data: {JsonConvert.SerializeObject(levelData)}");

        foreach (var level in levelData)
        {
            int levelIndex = level.Key;
            List<float> maxMouthOpenings = level.Value;

            DD_Lines lineToUse = lines[0]; // Use the first available line

            if (lineToUse == null)
            {
                Debug.LogError("❌ lineToUse is null!");
                return;
            }

            Debug.Log($"🔹 Level {levelIndex} - Adding Points");

            for (int attemptIndex = 0; attemptIndex < maxMouthOpenings.Count; attemptIndex++)
            {
                float yValue = maxMouthOpenings[attemptIndex];
                float xValue = attemptIndex + 1;

                Debug.Log($"✅ Adding Point to Graph: Level {levelIndex}, Attempt {attemptIndex + 1}, X={xValue}, Y={yValue}");
                lineToUse.AddPoint(new Vector2(xValue, yValue)); // Add the point dynamically
            }
        }
    }

    public async void OnLoadDataPressed()
    {
        // Show the loading spinner while the data is being fetched
        loadingSpinner.gameObject.SetActive(true);

        Debug.Log("🔄 Loading Data...");
        Dictionary<int, List<float>> levelData = await LoadProgressData();
        UpdateGraph(levelData); // Update the graph with the loaded data

        // Hide the loading spinner once data is loaded
        loadingSpinner.gameObject.SetActive(false);
    }

    public async void OnLevelChanged(int selectedLevel)
    {
        Debug.Log($"📌 Level Changed to: {selectedLevel + 1}");

        // Fetch the complete data again
        Dictionary<int, List<float>> levelData = await LoadProgressData();

        // Fetch data only for the selected level (index is zero-based)
        if (levelData.ContainsKey(selectedLevel + 1))
        {
            Dictionary<int, List<float>> filteredData = new Dictionary<int, List<float>>
            {
                { selectedLevel + 1, levelData[selectedLevel + 1] }
            };
            UpdateGraph(filteredData); // Update the graph with filtered data for the selected level
        }
    }
}

// Model class to deserialize data from Cloud Save
[System.Serializable]
public class LevelAttemptData
{
    public float MaxMouthOpening;
}
