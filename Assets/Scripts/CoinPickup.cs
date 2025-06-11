using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Visual Effects")]
    public float rotationSpeed = 90f; // Degrees per second
    
    [Header("Collection Settings")]
    public string playerTag = "Player"; // Tag to identify player
    
    private CoinSpawner coinSpawner;
    private bool isCollected = false;

    private void Start()
    {
        // Find the coin spawner to notify when collected
        coinSpawner = FindObjectOfType<CoinSpawner>();
        
    }

    private void Update()
    {
        if (!isCollected)
        {
            // Rotate the coin for visual appeal
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player collected the coin
        if (!isCollected && other.CompareTag(playerTag))
        {
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        if (isCollected) return; // Prevent double collection
        
        isCollected = true;
        
        // Get coin value from spawner
        int coinValue = coinSpawner != null ? coinSpawner.GetCoinValue() : 75;
        
        // Add coins to player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(coinValue);
            Debug.Log($"Collected {coinValue} coins!");
        }
        
        
        // Notify spawner that this coin was collected
        if (coinSpawner != null)
        {
            coinSpawner.OnCoinCollected(gameObject);
        }
        
        // Hide the visual immediately
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        // Destroy after brief delay
        Destroy(gameObject, 0.1f);
    }

} 