using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public GameManager gameManager;
    public int coinValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameManager.totalCoins += coinValue;
        Destroy(gameObject);
    }
}
