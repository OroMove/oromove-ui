using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.FaceLandmarkDetection; // Import the namespace


public class Player : MonoBehaviour
{
    public float moveSpeed;
    private Rigidbody2D rb;
    private GameManager gameManager;
    private string lipPosition = "CENTER";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>(); // Get GameManager reference

        // Subscribe to FaceLandmarkerRunner event
        FaceLandmarkerRunner.OnLipPositionChanged += UpdateLipPosition;
    }

    void Update()
    {
        if (gameManager != null && gameManager.gameOver)
        {
            rb.linearVelocity = Vector2.zero; // Stop movement if game is over
            return;
        }

        if (gameManager != null && gameManager.GameStarted()) // Check if game has started
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        if (lipPosition == "LEFT")
        {
            rb.AddForce(Vector2.left * moveSpeed);
        }
        else if (lipPosition == "RIGHT")
        {
            rb.AddForce(Vector2.right * moveSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop movement when lips are centered
        }
    }

    private void UpdateLipPosition(string newPosition)
    {
        lipPosition = newPosition;
    }

    private void OnDestroy()
    {
        FaceLandmarkerRunner.OnLipPositionChanged -= UpdateLipPosition;
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Block")) // Check if the collided object has the "Block" tag
        {
            if (gameManager != null)
            {
                gameManager.GameOver(); // Call GameOver() in the GameManager
            }
            else
            {
                Debug.LogError("GameManager not found!"); // Log an error if GameManager is missing
            }
        }
    }
}