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
    public GameObject blockPrefab;
    public float maxX;
    public Transform spawnPoint;
    public float spawnRate = 1.5f;
    public int blocksPerLevel = 10;

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

    private bool gameStarted = false;
    private bool gameOver = false;
    private int score = 0;
    private int highestScore = 0;
    private float startTime;
    private int blocksSpawned = 0;
    private int activeBlocks = 0; // Track number of active blocks

    // Public properties to access gameStarted and gameOver
    public bool IsGameStarted => gameStarted;
    public bool IsGameOver => gameOver;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await SignIn();
        await LoadHighestScore();

        timeText.text = "";
        gameOverPanel?.SetActive(false);
        levelCompletePanel?.SetActive(false);  // Ensure the level complete panel is hidden initially
    }

    async Task SignIn()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
    }

    void Update()
    {
        if (gameOver) return;
        if (gameStarted) UpdateTime();
    }

    public void StartGame()
    {
        Debug.Log("Game Started!");
        startTime = Time.time;
        startPanel?.SetActive(false);
        gameStarted = true;
        Debug.Log("Game panel hidden, starting game...");
        StartCoroutine(SpawnBlocks());
    }

    IEnumerator SpawnBlocks()  // Corrected to non-generic IEnumerator
    {
        while (!gameOver && blocksSpawned < blocksPerLevel)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.x = Random.Range(-maxX, maxX);
            Instantiate(blockPrefab, spawnPos, Quaternion.identity);
            blocksSpawned++;
            activeBlocks++; // Increase active block count
            yield return new WaitForSeconds(spawnRate);
        }
    }

    public void IncreaseScore()
    {
        if (gameOver) return;
        score++;
        scoreText.text = "Score: " + score;
    }

    private void UpdateTime()
    {
        float elapsedTime = Time.time - startTime;
        timeText.text = $"{(int)elapsedTime / 60:00}:{(elapsedTime % 60):00}";
    }

    public async void GameOver()
    {
        Debug.Log("Game Over!");
        gameOver = true;
        gameStarted = false;
        StopAllCoroutines();

        scoreValueText.text = "Score: " + score;

        if (score > highestScore)
        {
            highestScore = score;
            highestScoreText.text = "New Top Score: " + highestScore;
            await SaveHighestScore(highestScore);
            Debug.Log($"Saving New Highest Score: {score}");
        }
        else
        {
            highestScoreText.text = "Top Score: " + highestScore;
        }

        DisplayGameOverInfo();  // Show info for game over
        gameOverPanel.SetActive(true);
    }

    private async Task LoadHighestScore()
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "highest_score" });
            if (data.ContainsKey("highest_score"))
            {
                highestScore = int.Parse(data["highest_score"].ToString());
                highestScoreText.text = "Top Score: " + highestScore;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load highest score: " + e.Message);
            highestScoreText.text = "Error loading score";
        }
    }

    private async Task SaveHighestScore(int newScore)
    {
        try
        {
            var data = new Dictionary<string, object> { { "highest_score", newScore } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);

            Debug.Log($"Successfully saved highest score: {newScore}");

            // Load again to confirm it's saved
            await LoadHighestScore();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save highest score: " + e.Message);
        }
    }

    public void BlockDestroyed()
    {
        if (gameOver) return; // Prevents execution if Game Over has already occurred

        activeBlocks--; // Reduce active block count
        Debug.Log("Block destroyed. Remaining: " + activeBlocks);

        if (activeBlocks <= 0 && blocksSpawned >= blocksPerLevel)
        {
            // Only show the Level Complete panel if the game hasn't ended
            if (!gameOver)
            {
                levelCompletePanel?.SetActive(true);
                StopAllCoroutines();
                gameOverPanel?.SetActive(false);
                DisplayLevelCompleteInfo();
            }
        }
    }

    private void DisplayLevelCompleteInfo()
    {
        // Display only the values without labels
        levelCompleteScoreText.text = score.ToString();
        levelCompleteTimeText.text = FormatTime(Time.time - startTime);
        levelCompleteHighestScoreText.text = highestScore.ToString();
    }

    private void DisplayGameOverInfo()
    {
        // Display only the values without labels
        gameOverScoreText.text = score.ToString();
        gameOverTimeText.text = FormatTime(Time.time - startTime);
        gameOverHighestScoreText.text = highestScore.ToString();
    }

    // Helper function to format time (without "Time: ")
    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void NextLevel()
    {
        SceneManager.LoadSceneAsync("OD_Level2");
    }

    public void CloseLevel()
    {
        SceneManager.LoadSceneAsync("OD_LevelSelection");
    }

    public void Home()
    {
        SceneManager.LoadSceneAsync("OD_MainMenu");
    }
}
