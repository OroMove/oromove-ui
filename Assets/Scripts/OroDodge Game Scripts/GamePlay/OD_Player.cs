using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.FaceLandmarkDetection; // Import the FaceLandmarkDetection namespace from Mediapipe

public class Player : MonoBehaviour
{
    // Speed at which the player moves
    public float moveSpeed;

    // Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;

    // Reference to the game manager that controls game logic
    private ODGameManager gameManager;

    // Default lip position (CENTER by default)
    private string lipPosition = "CENTER";

    // Flag to track if the player has won the game
    private bool hasWon = false;

    // Sprite references for different player states (e.g., skating, relaxing, victory, sad)
    public Sprite skatingLeftSprite;
    public Sprite skatingRightSprite;
    public Sprite relaxingSprite;
    public Sprite victorySprite;
    public Sprite sadSprite;

    // SpriteRenderer to modify the player's sprite based on their state
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Initializing components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get SpriteRenderer component attached to the player
        gameManager = FindObjectOfType<ODGameManager>(); // Find the GameManager object in the scene

        // Check if GameManager exists, otherwise log an error
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found! Ensure the game manager is present in the scene.");
        }

        // Subscribe to the event for face landmark updates (lip position changes)
        FaceLandmarkerRunnerLip.OnLipPositionChanged += UpdateLipPosition;
    }

    void Update()
    {
        // If GameManager is not set or the player has already won, skip further updates
        if (gameManager == null || hasWon) return;

        // If the game is over, stop movement and set a sad sprite
        if (gameManager.IsGameOver)
        {
            StopPlayer(); // Stop player movement immediately
            spriteRenderer.sprite = sadSprite; // Change sprite to sad
            return;
        }

        // If the game has started, update the player's movement
        if (gameManager.IsGameStarted)
        {
            MovePlayer();
        }
    }

    // Method to control the player's movement based on lip position
    private void MovePlayer()
    {
        // Debugging output to track the current lip position
        Debug.Log("Lip Position: " + lipPosition);

        // Move the player based on lip position
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
            StopPlayer(); // Stop player if lips are centered
            spriteRenderer.sprite = relaxingSprite; // Change sprite to relaxing
        }
    }

    // Event handler for updating lip position when it's changed by the FaceLandmarker
    private void UpdateLipPosition(string newPosition)
    {
        // Skip updating if the player has already won
        if (hasWon) return;

        // Check if the lip position has changed
        if (lipPosition != newPosition)
        {
            Debug.Log("Lip Position Changed: " + newPosition); // Log the new lip position for debugging
            lipPosition = newPosition; // Update the lip position
        }
    }

    // Method to stop player movement (used when game is over or player is idle)
    private void StopPlayer()
    {
        rb.linearVelocity = Vector2.zero; // Stop player movement by setting velocity to zero
    }

    // Ensure to unsubscribe from the event when the object is destroyed to prevent memory leaks
    private void OnDestroy()
    {
        FaceLandmarkerRunnerLip.OnLipPositionChanged -= UpdateLipPosition; // Unsubscribe from the event
    }

    // Also unsubscribe from the event when the object is disabled (important if object is reused)
    private void OnDisable()
    {
        FaceLandmarkerRunnerLip.OnLipPositionChanged -= UpdateLipPosition; // Unsubscribe to avoid any unintended behavior
    }

    // Handle collision events (e.g., when the player collides with a "Block" object)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player collided with an object tagged as "Block"
        if (collision.gameObject.CompareTag("Block"))
        {
            // If the game manager exists and the player hasn't already won, call GameOver()
            if (gameManager != null && !hasWon)
            {
                gameManager.GameOver(); // Trigger game over logic
                spriteRenderer.sprite = sadSprite; // Set the sprite to sad when the player loses
            }
        }
    }

    // Call this method when the player wins the game
    public void WinGame()
    {
        hasWon = true; // Mark the player as having won
        rb.linearVelocity = Vector2.zero; // Stop movement by setting velocity to zero
        spriteRenderer.sprite = victorySprite; // Set the victory sprite
        Debug.Log("Player has won! Victory sprite set."); // Log the win event for debugging
    }
}
