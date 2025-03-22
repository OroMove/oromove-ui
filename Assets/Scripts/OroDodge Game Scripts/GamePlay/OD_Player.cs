using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.FaceLandmarkDetection; // Import the namespace

public class Player : MonoBehaviour
{
    public float moveSpeed;
    private Rigidbody2D rb;
    private ODGameManager gameManager;
    private string lipPosition = "CENTER"; // Default value is "CENTER"
    private bool hasWon = false; // Track if player has won

    // Sprite references for different player states
    public Sprite skatingLeftSprite;
    public Sprite skatingRightSprite;
    public Sprite relaxingSprite;
    public Sprite victorySprite;
    public Sprite sadSprite;

    private SpriteRenderer spriteRenderer; // To change the sprite of the player

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
        gameManager = FindObjectOfType<ODGameManager>(); // Get GameManager reference

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
        }

        // Subscribe to FaceLandmarkerRunner event
        FaceLandmarkerRunnerLip.OnLipPositionChanged += UpdateLipPosition;
    }

    void Update()
    {
        if (gameManager == null || hasWon) return;

        // Check if the game is over, if so stop movement and set sad sprite
        if (gameManager.IsGameOver)
        {
            rb.linearVelocity = Vector2.zero; // Stop movement if game is over
            spriteRenderer.sprite = sadSprite; // Set sprite to sad when the player loses
            return;
        }

        // Check if the game has started, if so start moving the player
        if (gameManager.IsGameStarted)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        // Debugging logs to check the current lip position and if the player is moving
        Debug.Log("Lip Position: " + lipPosition); // Debug log to check lip position

        // Update sprite and movement based on lip position
        if (lipPosition == "LEFT")
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y); // Move left
            spriteRenderer.sprite = skatingLeftSprite; // Change sprite to skating left
        }
        else if (lipPosition == "RIGHT")
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y); // Move right
            spriteRenderer.sprite = skatingRightSprite; // Change sprite to skating right
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop movement when lips are centered
            spriteRenderer.sprite = relaxingSprite; // Change sprite to relaxing
        }
    }

    private void UpdateLipPosition(string newPosition)
    {
        if (hasWon) return; // Prevent updating movement if the player has won

        Debug.Log("Lip Position Changed: " + newPosition); // Debug log to track lip position changes
        lipPosition = newPosition; // Update lip position when the event is triggered
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed
        FaceLandmarkerRunnerLip.OnLipPositionChanged -= UpdateLipPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collision with a "Block"
        if (collision.gameObject.CompareTag("Block"))
        {
            if (gameManager != null && !hasWon)
            {
                gameManager.GameOver(); // Call GameOver() in the GameManager
                spriteRenderer.sprite = sadSprite; // Set sprite to sad when player loses
            }
        }
    }

    // Call this function to set the victory sprite when the player wins
    public void WinGame()
    {
        hasWon = true; // Mark player as having won
        rb.linearVelocity = Vector2.zero; // Stop movement
        spriteRenderer.sprite = victorySprite; // Set sprite to victory
        Debug.Log("Player has won! Victory sprite set.");
    }
}
