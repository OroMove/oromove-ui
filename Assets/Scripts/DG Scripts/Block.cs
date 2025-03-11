using UnityEngine;

public class Block : MonoBehaviour
{
    private bool scored = false;
    private GameManager gameManager;

    void Start()
    {
        // Find the GameManager in the scene if it's not assigned
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    void Update()
    {
        if (!scored && transform.position.y < -3.2609f)
        {
            if (gameManager != null) // Prevents NullReferenceException
            {
                gameManager.IncreaseScore(); // Increase the score when the block passes the threshold
                scored = true;
            }
            else
            {
                Debug.LogError("GameManager reference is missing in Block script!");
            }
        }

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
}
