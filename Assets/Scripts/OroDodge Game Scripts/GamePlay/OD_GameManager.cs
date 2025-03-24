using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class ODGameManager : MonoBehaviour
{
    // Prefabs and GameObjects for game elements
    public GameObject blockPrefab;
    public float maxX;  // Maximum spawn range on X-axis
    public Transform spawnPoint;  // The point from which blocks will spawn
    public float spawnRate = 1.5f;  // Time interval between block spawns
    public int blocksPerLevel;  // Number of blocks to spawn per level

    // Panels and UI elements for displaying scores, game status, and more
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;  // Add level complete panel
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI highestScoreText;
    public TextMeshProUGUI scoreValueText;

    // TextMeshProUGUI for Game Over and Level Complete Panels
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverTimeText;
    public TextMeshProUGUI gameOverHighestScoreText;
    public TextMeshProUGUI levelCompleteScoreText;
    public TextMeshProUGUI levelCompleteTimeText;
    public TextMeshProUGUI levelCompleteHighestScoreText;

    // Game state tracking variables
    private bool gameStarted = false;  // Flag to track whether the game has started
    private bool gameOver = false;  // Flag to track if the game is over
    private int score = 0;  // Current score of the player
    private int highestScore = 0;  // Highest score for the current level
    private float startTime;  // Time when the game started
    private int blocksSpawned = 0;  // Number of blocks spawned in the current game
    private int activeBlocks = 0;  // Number of blocks currently active in the game

    private string currentLevelName;  // Store the name of the current level for high score tracking

    // Public properties to access game state
    public bool IsGameStarted => gameStarted;
    public bool IsGameOver => gameOver;

    async void Start()
    {
        // Initialize Unity Game Services and sign in to authentication
        await InitializeUGS();
        await SignIn();

        // Get the current level name and load its highest score
        currentLevelName = SceneManager.GetActiveScene().name;
        await LoadHighestScore(currentLevelName);

        // Initialize UI elements
        timeText.text = "";
        gameOverPanel?.SetActive(false);
        levelCompletePanel?.SetActive(false);  // Ensure the level complete panel is hidden initially
    }

    // Initialize Unity Game Services for CloudSave and Authentication
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
        catch (System.Exception e)
        {
            Debug.LogError($"[InitializeUGS] Initialization failed: {e.Message}");
            throw;
        }
    }

    // Sign in the player anonymously to Unity Authentication
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
            catch (System.Exception e)
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
        // Skip updating if the game is over
        if (gameOver) return;

        // Update time display if the game has started
        if (gameStarted) UpdateTime();
    }

    // Start the game and begin spawning blocks
    public void StartGame()
    {
        Debug.Log("Game Started!");
        startTime = Time.time;  // Record the time when the game starts
        startPanel?.SetActive(false);  // Hide the start panel
        gameStarted = true;  // Set gameStarted flag to true
        Debug.Log("Game panel hidden, starting game...");
        StartCoroutine(SpawnBlocks());
    }

    // Coroutine to spawn blocks at the specified spawn rate
    IEnumerator SpawnBlocks()
    {
        while (!gameOver && blocksSpawned < blocksPerLevel)
        {
            // Randomize the spawn position along the X-axis
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.x = Random.Range(-maxX, maxX);
            Instantiate(blockPrefab, spawnPos, Quaternion.identity);  // Spawn the block prefab
            blocksSpawned++;  // Increment the blocks spawned count
            activeBlocks++;  // Increment the active blocks count
            yield return new WaitForSeconds(spawnRate);  // Wait for the next spawn
        }
    }

    // Increase the score when a block is destroyed
    public void IncreaseScore()
    {
        if (gameOver) return;  // Prevent score increase if the game is over
        score++;  // Increment score
        scoreText.text = "Score: " + score;  // Update the score UI
    }

    // Update the time display to show elapsed time
    private void UpdateTime()
    {
        float elapsedTime = Time.time - startTime;
        timeText.text = $"{(int)elapsedTime / 60:00}:{(elapsedTime % 60):00}";  // Format time in MM:SS
    }

    // Handle game over logic and save the highest score
    public async void GameOver()
    {
        Debug.Log("Game Over!");
        gameOver = true;  // Set the game over flag
        gameStarted = false;  // Set gameStarted flag to false
        StopAllCoroutines();  // Stop the block spawning coroutine

        // Display final score and save highest score
        scoreValueText.text = "Score: " + score;

        // Update highest score if the current score exceeds the previous highest score
        if (score > highestScore)
        {
            highestScore = score;
            highestScoreText.text = "New Top Score: " + highestScore;
            await SaveHighestScore(currentLevelName, highestScore);  // Save the new highest score
            Debug.Log($"Saving New Highest Score: {score}");
        }
        else
        {
            highestScoreText.text = "Top Score: " + highestScore;  // Display the top score
        }

        // Save relevant game data (score, time, etc.)
        await SaveGameData();

        // Display game over information
        DisplayGameOverInfo();
        gameOverPanel.SetActive(true);  // Show the game over panel
    }

    // Save game data like score, time taken, etc., to CloudSave
    private async Task SaveGameData()
    {
        try
        {
            var gameData = new Dictionary<string, object>
            {
                { currentLevelName + "_score", score },
                { currentLevelName + "_highest_score", highestScore },
                { currentLevelName + "_time_taken", Time.time - startTime },
                { currentLevelName + "_spawn_rate", spawnRate },
                { currentLevelName + "_blocks_allocated", blocksPerLevel }
            };
            await CloudSaveService.Instance.Data.ForceSaveAsync(gameData);  // Save data asynchronously

            Debug.Log("[SaveGameData] Game data saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveGameData] Failed to save game data: {e.Message}");
        }
    }

    // Load the highest score for the current level from CloudSave
    private async Task LoadHighestScore(string levelName)
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { levelName + "_highest_score" });
            if (data.ContainsKey(levelName + "_highest_score"))
            {
                highestScore = int.Parse(data[levelName + "_highest_score"].ToString());
                highestScoreText.text = "Top Score: " + highestScore;  // Display the loaded highest score
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load highest score: " + e.Message);
            highestScoreText.text = "Error loading score";  // Display error message if loading fails
        }
    }

    // Save the new highest score to CloudSave
    private async Task SaveHighestScore(string levelName, int newScore)
    {
        try
        {
            var data = new Dictionary<string, object> { { levelName + "_highest_score", newScore } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);  // Save the highest score asynchronously

            Debug.Log($"Successfully saved highest score: {newScore}");

            // Reload the highest score to confirm it's saved
            await LoadHighestScore(levelName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save highest score: " + e.Message);
        }
    }

    // Handle block destruction logic
    public void BlockDestroyed()
    {
        if (gameOver) return; // Prevents execution if Game Over has already occurred

        activeBlocks--; // Reduce active block count
        Debug.Log("Block destroyed. Remaining: " + activeBlocks);

        // Update highest score if current score exceeds the stored highest score
        if (score > highestScore)
        {
            highestScore = score; // Update the highest score
            highestScoreText.text = "New Top Score: " + highestScore; // Update the UI with new highest score
                                                                      // Save the new highest score to the cloud
            SaveHighestScore(currentLevelName, highestScore);
            Debug.Log($"New highest score: {highestScore} saved!");
        }

        if (activeBlocks <= 0 && blocksSpawned >= blocksPerLevel)
        {
            // Only show the Level Complete panel if the game has ended
            if (!gameOver)
            {
                StopAllCoroutines();
                levelCompletePanel?.SetActive(true);
                gameOverPanel?.SetActive(false);
                DisplayLevelCompleteInfo();
            }
        }
    }


    // Display information when the level is completed
    private void DisplayLevelCompleteInfo()
    {
        levelCompleteScoreText.text = score.ToString();  // Show current score
        levelCompleteTimeText.text = FormatTime(Time.time - startTime);  // Show time taken
        levelCompleteHighestScoreText.text = highestScore.ToString();  // Show highest score
    }

    // Display information when the game is over
    private void DisplayGameOverInfo()
    {
        gameOverScoreText.text = score.ToString();  // Show final score
        gameOverTimeText.text = FormatTime(Time.time - startTime);  // Show time taken
        gameOverHighestScoreText.text = highestScore.ToString();  // Show highest score
    }

    // Helper function to format time (without "Time: ")
    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";  // Format time as MM:SS
    }

    // Navigate to the main menu
    public void Home()
    {
        SceneManager.LoadSceneAsync("OD_MainMenu");  // Load the main menu scene
    }

    // Load the next level by its ID
    public void NextLevel(int levelID)
    {
        string levelName = "OD_Level_0" + levelID;  // Build the level name from ID
        SceneManager.LoadScene(levelName);  // Load the specified level
    }

    // Close the current level and return to the level selection screen
    public void CloseLevel()
    {
        SceneManager.LoadSceneAsync("OD_LevelSelection");  // Load the level selection scene
    }

    // Restart the current game
    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);  // Reload the current scene
    }

    // Quit the game
    public void ExitGame()
    {
        Application.Quit();  // Quit the application
    }
}
