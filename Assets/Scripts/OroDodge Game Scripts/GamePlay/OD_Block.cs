using UnityEngine;

public class Block : MonoBehaviour
{
    private bool scored = false;
    private ODGameManager gameManager;
    public float fallSpeedMultiplier = 0.1f; // Adjust this value to change speed

    private Rigidbody2D rb;

    void Start()
    {
        gameManager = FindObjectOfType<ODGameManager>(); // Auto-find GameManager if not assigned
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale *= fallSpeedMultiplier; // Increase gravity to fall faster
        }
    }

    void Update()
    {
        if (!scored && transform.position.y < -3.26f)
        {
            if (gameManager != null)
            {
                gameManager.IncreaseScore();
                scored = true; // Prevent double scoring
            }
            else
            {
                Debug.LogError("GameManager reference is missing in Block script!");
            }
        }

        if (transform.position.y < -6f) // Destroy off-screen blocks
        {
            if (gameManager != null)
            {
                gameManager.BlockDestroyed(); // Notify GameManager before destruction
            }
            Destroy(gameObject);
        }
    }
}
