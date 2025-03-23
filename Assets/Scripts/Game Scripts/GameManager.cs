using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

[System.Serializable]
public class AttemptData
{
    public float[] mouthOpeningDistances;
    public float[] speeds;
    public float finalDistance;
    public float timeTaken;
    public float averageSpeed;
    public float maxSpeed;
    public float minSpeed;
    public float maxMouthOpening;
    public float minMouthOpening;
}

[System.Serializable]
public class LevelProgress
{
    public int levelId;
    public int totalAttempts;
    public Dictionary<int, AttemptData> attempts;

    public LevelProgress()
    {
        attempts = new Dictionary<int, AttemptData>();
    }
}

public class GameManager : MonoBehaviour
{
    private Dictionary<int, LevelProgress> levelProgressData = new Dictionary<int, LevelProgress>();
    private bool isGameInitialized = false;
    private bool isGamePaused = false;

    public GameObject gameOverScreen;
    public GameObject levelCompletePanel;
    public GameObject pauseMenu; // Add a reference to your pause menu UI
    public CarController car;
    public Text coinCounterText;
    public Text distanceText;
    public Text highestDistanceText;
    public Text finalDistanceText;
    public Text finalSpeedText;
    public Text finalTimeText;

    public int totalCoins;
    public Transform playerTransform;
    private Vector2 startingPosition;
    private float currentDistance;
    private float highestDistance;
    private float startTime;

    private Task pendingSaveTask = null;

    private async void Start()
    {
        Debug.Log("[GameManager] Starting initialization sequence...");
        await InitializeUGS();
        await SignIn();

        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"[GameManager] Current scene level index: {currentLevel}");

        startingPosition = playerTransform.position;
        startTime = Time.time;

        // Load existing progress before initializing new data
        await LoadAndSyncProgress(currentLevel);

        if (!levelProgressData.ContainsKey(currentLevel))
        {
            Debug.Log($"[GameManager] No progress found after load, initializing new progress for Level {currentLevel}");
            levelProgressData[currentLevel] = new LevelProgress
            {
                levelId = currentLevel,
                totalAttempts = 0
            };
        }

        Debug.Log($"[GameManager] Initialization complete. Current total attempts: {levelProgressData[currentLevel].totalAttempts}");
        isGameInitialized = true;
        UpdateUI();
    }

    // Add Pause and Resume methods
    public void PauseGame()
    {
        if (!isGamePaused)
        {
            Debug.Log("[GameManager] Pausing game");
            Time.timeScale = 0f;
            isGamePaused = true;
            pauseMenu.SetActive(true); // Show the pause menu
        }
    }

    public void ResumeGame()
    {
        if (isGamePaused)
        {
            Debug.Log("[GameManager] Resuming game");
            Time.timeScale = 1f;
            isGamePaused = false;
            pauseMenu.SetActive(false); // Hide the pause menu
        }
    }

    private async Task LoadAndSyncProgress(int levelId)
    {
        try
        {
            Debug.Log($"[LoadAndSyncProgress] Starting load for Level {levelId}");

            // Get all existing keys first
            var allKeys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
            List<string> allKeysList = allKeys.Select(k => k.Key).ToList(); // Convert ItemKey to string

            Debug.Log($"[LoadAndSyncProgress] Total keys in cloud save: {allKeysList.Count}");

            // Get all keys for this level
            var levelKeys = allKeysList.Where(k => k.StartsWith($"Level_{levelId}_")).ToList();
            Debug.Log($"[LoadAndSyncProgress] Keys for Level {levelId}: {string.Join(", ", levelKeys)}");

            if (levelKeys.Count > 0)
            {
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>(levelKeys));
                Debug.Log($"[LoadAndSyncProgress] Loaded {result.Count} data entries");

                // Initialize level progress if needed
                if (!levelProgressData.ContainsKey(levelId))
                {
                    levelProgressData[levelId] = new LevelProgress { levelId = levelId };
                }

                // Load metadata
                string metaKey = $"Level_{levelId}_Meta";
                if (result.TryGetValue(metaKey, out var metaData))
                {
                    string metaJson = JsonConvert.SerializeObject(metaData.Value);
                    Debug.Log($"[LoadAndSyncProgress] Metadata found: {metaJson}");

                    var metaDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(metaJson);
                    if (metaDict != null && metaDict.ContainsKey("TotalAttempts"))
                    {
                        levelProgressData[levelId].totalAttempts = Convert.ToInt32(metaDict["TotalAttempts"]);
                        Debug.Log($"[LoadAndSyncProgress] Set total attempts to: {levelProgressData[levelId].totalAttempts}");
                    }
                }

                // Load all attempts
                foreach (var key in levelKeys.Where(k => k.Contains("_Attempt_")))
                {
                    if (result.TryGetValue(key, out var attemptData))
                    {
                        string attemptJson = JsonConvert.SerializeObject(attemptData.Value);
                        Debug.Log($"[LoadAndSyncProgress] Loading attempt data: {key} -> {attemptJson}");

                        var attemptDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(attemptJson);
                        if (attemptDict != null)
                        {
                            // Parse attempt number from key
                            string[] parts = key.Split('_');
                            if (int.TryParse(parts[parts.Length - 1], out int attemptNumber))
                            {
                                var attempt = new AttemptData
                                {
                                    finalDistance = Convert.ToSingle(attemptDict["FinalDistance"]),
                                    timeTaken = Convert.ToSingle(attemptDict["TimeTaken"]),
                                    averageSpeed = Convert.ToSingle(attemptDict["AverageSpeed"]),
                                    maxSpeed = Convert.ToSingle(attemptDict["MaxSpeed"]),
                                    minSpeed = Convert.ToSingle(attemptDict["MinSpeed"]),
                                    maxMouthOpening = Convert.ToSingle(attemptDict["MaxMouthOpening"]),
                                    minMouthOpening = Convert.ToSingle(attemptDict["MinMouthOpening"]),
                                    mouthOpeningDistances = ((Newtonsoft.Json.Linq.JArray)attemptDict["MouthOpeningDistances"]).Select(x => (float)x).ToArray(),
                                    speeds = ((Newtonsoft.Json.Linq.JArray)attemptDict["Speeds"]).Select(x => (float)x).ToArray()
                                };

                                levelProgressData[levelId].attempts[attemptNumber] = attempt;
                                Debug.Log($"[LoadAndSyncProgress] Loaded attempt {attemptNumber} successfully");
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"[LoadAndSyncProgress] No existing data found for Level {levelId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoadAndSyncProgress] Error: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    private async Task SaveLevelProgress(int levelId, LevelProgress progress)
    {
        Debug.Log($"[SaveLevelProgress] Saving Level {levelId}, Attempt {progress.totalAttempts}");
        try
        {
            var currentAttempt = progress.attempts[progress.totalAttempts];

            // Create attempt data
            var attemptData = new Dictionary<string, object>
            {
                ["MouthOpeningDistances"] = currentAttempt.mouthOpeningDistances,
                ["Speeds"] = currentAttempt.speeds,
                ["FinalDistance"] = currentAttempt.finalDistance,
                ["TimeTaken"] = currentAttempt.timeTaken,
                ["AverageSpeed"] = currentAttempt.averageSpeed,
                ["MaxSpeed"] = currentAttempt.maxSpeed,
                ["MinSpeed"] = currentAttempt.minSpeed,
                ["MaxMouthOpening"] = currentAttempt.maxMouthOpening,
                ["MinMouthOpening"] = currentAttempt.minMouthOpening,
                ["Timestamp"] = DateTime.UtcNow.ToString("o")
            };

            // Create save data dictionary
            var saveData = new Dictionary<string, object>
            {
                [$"Level_{levelId}_Meta"] = new Dictionary<string, object>
                {
                    ["LevelId"] = levelId,
                    ["TotalAttempts"] = progress.totalAttempts,
                    ["LastUpdated"] = DateTime.UtcNow.ToString("o")
                },
                [$"Level_{levelId}_Attempt_{progress.totalAttempts}"] = attemptData
            };

            Debug.Log($"[SaveLevelProgress] Preparing to save - Meta: {JsonConvert.SerializeObject(saveData[$"Level_{levelId}_Meta"])}");

            pendingSaveTask = CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
            await pendingSaveTask;

            Debug.Log($"[SaveLevelProgress] Save completed for Level {levelId}, Attempt {progress.totalAttempts}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLevelProgress] Error: {e.Message}\nStack trace: {e.StackTrace}");
        }
        finally
        {
            pendingSaveTask = null;
        }
    }

    private async void UpdateLevelProgress()
    {
        if (!isGameInitialized)
        {
            Debug.LogError("[UpdateLevelProgress] Cannot update before initialization");
            return;
        }

        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"[UpdateLevelProgress] Updating progress for Level {currentLevel}");

        var levelProgress = levelProgressData[currentLevel];
        int previousAttempts = levelProgress.totalAttempts;
        levelProgress.totalAttempts++;

        Debug.Log($"[UpdateLevelProgress] Incrementing attempts: {previousAttempts} -> {levelProgress.totalAttempts}");

        var attemptData = CreateAttemptData();
        levelProgress.attempts[levelProgress.totalAttempts] = attemptData;

        await SaveLevelProgress(currentLevel, levelProgress);
    }

    private AttemptData CreateAttemptData()
    {
        List<float> mouthDistances = car.GetNonZeroMouthOpeningDistances();
        float[] speeds = new float[mouthDistances.Count];
        float elapsedTime = Time.time - startTime;

        for (int i = 0; i < mouthDistances.Count; i++)
        {
            speeds[i] = currentDistance * mouthDistances[i] / elapsedTime;
        }

        var data = new AttemptData
        {
            mouthOpeningDistances = mouthDistances.ToArray(),
            speeds = speeds,
            finalDistance = currentDistance,
            timeTaken = elapsedTime,
            averageSpeed = currentDistance / elapsedTime,
            maxSpeed = speeds.Length > 0 ? Mathf.Max(speeds) : 0,
            minSpeed = speeds.Length > 0 ? Mathf.Min(speeds) : 0,
            maxMouthOpening = mouthDistances.Count > 0 ? Mathf.Max(mouthDistances.ToArray()) : 0,
            minMouthOpening = mouthDistances.Count > 0 ? Mathf.Min(mouthDistances.ToArray()) : 0
        };

        Debug.Log($"[CreateAttemptData] Created attempt data - Distance: {data.finalDistance:F2}m, Time: {data.timeTaken:F2}s, Speed: {data.averageSpeed:F2}m/s");
        return data;
    }

    private async Task InitializeUGS()
    {
        Debug.Log("[InitializeUGS] Starting Unity Game Services initialization...");
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Debug.Log("[InitializeUGS] Services already initialized");
            return;
        }

        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("[InitializeUGS] Services initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"[InitializeUGS] Initialization failed: {e.Message}");
            throw;
        }
    }

    private async Task SignIn()
    {
        Debug.Log("[SignIn] Starting authentication...");
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[SignIn] Successfully signed in. Player ID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignIn] Authentication failed: {e.Message}");
                throw;
            }
        }
        else
        {
            Debug.Log("[SignIn] Already signed in");
        }
    }

    void Update()
    {
        if (!isGameInitialized || isGamePaused) return;

        CheckGameOver();
        CalculateDistanceTravelled();
        UpdateUI();
    }

    private void CheckGameOver()
    {
        if (car.fuelLevel <= 0)
        {
            Debug.Log("[CheckGameOver] Fuel depleted, ending game");
            EndGameInstantly();
        }
    }

    public void EndGameInstantly()
    {
        if (!gameOverScreen.activeSelf && !levelCompletePanel.activeSelf)
        {
            Debug.Log("[EndGameInstantly] Displaying game over screen");
            Time.timeScale = 0f;
            gameOverScreen.SetActive(true);

            UpdateFinalStats();
            UpdateLevelProgress();
        }
    }

    public void LevelComplete()
    {
        if (!levelCompletePanel.activeSelf && !gameOverScreen.activeSelf)
        {
            Debug.Log("[LevelComplete] Displaying level complete screen");
            levelCompletePanel.SetActive(true);

            UpdateFinalStats();
            UpdateLevelProgress();
        }
    }

    private void UpdateFinalStats()
    {
        float elapsedTime = Time.time - startTime;
        finalDistanceText.text = $"Distance: {currentDistance:F0} m";
        highestDistanceText.text = $"Highest Distance: {highestDistance:F0} m";
        finalSpeedText.text = $"Speed: {currentDistance / elapsedTime:F2} m/s";
        finalTimeText.text = $"Time: {elapsedTime:F2} s";
    }

    public void RestartGame()
    {
        StartCoroutine(RestartAfterSave());
    }

    private IEnumerator RestartAfterSave()
    {
        Debug.Log("[RestartAfterSave] Preparing to restart game...");
        if (pendingSaveTask != null)
        {
            Debug.Log("[RestartAfterSave] Waiting for pending save to complete...");
            yield return new WaitUntil(() => pendingSaveTask.IsCompleted);
        }

        Time.timeScale = 1f;
        Debug.Log("[RestartAfterSave] Loading scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateUI()
    {
        coinCounterText.text = totalCoins.ToString();
        distanceText.text = $"{currentDistance:F0} m";
        highestDistanceText.text = $"Highest Distance: {highestDistance:F0} m";
    }

    private void CalculateDistanceTravelled()
    {
        currentDistance = Mathf.Max(0, playerTransform.position.x - startingPosition.x);
        highestDistance = Mathf.Max(highestDistance, currentDistance);
    }

    public void ExitGame()
    {
        Debug.Log("Back to Main Menu");
        SceneManager.LoadSceneAsync("HillClimberGameMenu");
    }
}