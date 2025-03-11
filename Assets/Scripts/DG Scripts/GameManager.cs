using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public GameObject block; // Prefab for the blocks to spawn
    public float maxX; // Maximum X position for spawning blocks
    public Transform spawnPoint; // Position where blocks will spawn
    public float spawnRate; // Rate at which blocks spawn
    public int blocksPerLevel = 10; // Number of blocks to spawn per level

    public GameObject tapText; // "Tap to Start" Text
    public GameObject gameOverPanel; // Game Over panel with buttons
    public TextMeshProUGUI scoreText; // UI Text for displaying score
    public TextMeshProUGUI timeText; // UI Text for displaying time
    public TextMeshProUGUI highestScoreText; // UI Text for highest score
    public TextMeshProUGUI scoreValueText; // UI Text for actual game-over score
    public GameObject trophyImage; // Trophy image (Assign in Inspector)

    private bool gameStarted = false;
    [HideInInspector] public bool gameOver = false;

    private int score = 0;
    private int highestScore = 0;
    private float startTime;
    private int blocksSpawnedInCurrentLevel = 0; // Tracks blocks spawned in the current level

    async void Start()
    {
        await UnityServices.InitializeAsync(); // Initialize UGS
        await SignIn(); // Authenticate the user
        await LoadHighestScore(); // Load the highest score from UGS

        if (trophyImage != null)
            trophyImage.SetActive(false); // Ensure the trophy is hidden at the start

        if (timeText != null) timeText.text = "";
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (tapText != null) tapText.SetActive(true);
    }

    async Task SignIn()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Anonymous login
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !gameStarted && !gameOver)
        {
            StartGame();
        }

        if (gameOver) return;

        if (gameStarted)
        {
            UpdateTime();
        }
    }

    public bool GameStarted()
    {
        return gameStarted;
    }

    private void StartGame()
    {
        Debug.Log("Game Started!");
        startTime = Time.time;
        StartSpawning();
        if (tapText != null) tapText.SetActive(false);
        gameStarted = true;
    }

    private void StartSpawning()
    {
        InvokeRepeating("SpawnBlock", 1f, spawnRate);
    }

    private void SpawnBlock()
    {
        if (gameOver || blocksSpawnedInCurrentLevel >= blocksPerLevel)
        {
            CancelInvoke("SpawnBlock"); // Stop spawning once the limit is reached
            return;
        }

        Vector3 spawnPos = spawnPoint.position;
        spawnPos.x = Random.Range(-maxX, maxX);
        Instantiate(block, spawnPos, Quaternion.identity);

        IncreaseScore(); // Increment score when a new block is spawned
        blocksSpawnedInCurrentLevel++;
    }

    public void IncreaseScore()
    {
        score++; // Increment the score
        UpdateScore(); // Update the score UI
    }

    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    private void UpdateTime()
    {
        float elapsedTime = Time.time - startTime;
        string minutes = ((int)elapsedTime / 60).ToString("00");
        string seconds = (elapsedTime % 60).ToString("00");

        if (timeText != null)
        {
            timeText.text = minutes + ":" + seconds;
        }
    }

    public async void GameOver()
    {
        Debug.Log("Game Over called!");
        gameOver = true;
        gameStarted = false;
        CancelInvoke("SpawnBlock");

        if (tapText != null) tapText.SetActive(false);

        // Update the UI for current score
        if (scoreValueText != null)
        {
            scoreValueText.text = "Score: " + score.ToString();
        }

        // Check and update the highest score
        if (score > highestScore)
        {
            highestScore = score; // Update the local highest score
            if (highestScoreText != null)
            {
                highestScoreText.text = "New Top Score: " + highestScore;
            }
            if (trophyImage != null)
            {
                trophyImage.SetActive(true); // Show the Trophy Image
            }
            await SaveHighestScore(highestScore);
        }
        else
        {
            if (highestScoreText != null)
            {
                highestScoreText.text = "Top Score: " + highestScore;
            }
            if (trophyImage != null)
            {
                trophyImage.SetActive(false); // Hide Trophy if no new high score
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private async Task LoadHighestScore()
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "highest_score" });
            if (data.ContainsKey("highest_score"))
            {
                highestScore = int.Parse(data["highest_score"].ToString());
                if (highestScoreText != null)
                {
                    highestScoreText.text = "Top Score: " + highestScore;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load highest score: " + e.Message);
        }
    }

    private async Task SaveHighestScore(int newScore)
    {
        try
        {
            var data = new Dictionary<string, object> { { "highest_score", newScore } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            Debug.Log("Highest Score Saved: " + newScore);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save highest score: " + e.Message);
        }
    }

    public void RetryGame()
    {
        Debug.Log("Retry clicked!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Game Exiting...");
        Application.Quit();
    }
}
