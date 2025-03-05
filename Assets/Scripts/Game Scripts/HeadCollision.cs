using UnityEngine;

public class HeadCollision : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>(); // Find the GameManager in the scene
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Fixed syntax error here
        {
            gameManager.EndGameInstantly(); // Trigger game over if head collides
        }
    }
}
