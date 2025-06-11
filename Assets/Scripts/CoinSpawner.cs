using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPickupPrefab; 
    public Transform player;

    [Header("Spawn Settings")]
    public float baseSpawnInterval = 8f;
    public int coinsPerSpawn = 75;
    public float[] laneXPositions = { -3.3f, 0f, 3.3f };
    public int maxCoinsOnScreen = 3;
    
    [Header("Timing Settings")]
    public float spawnTimingVariability = 2f;
    public float coinLifetime = 20f; 
    
    [Header("Difficulty Scaling")]
    public bool enableDifficultyScaling = true;
    public float difficultyIncreaseDistance = 1500f; // Every 1.5km distance
    public float spawnRateIncreasePerLevel = 0.4f; // Reduce interval by 0.4s per level
    public float minSpawnInterval = 4f; // Never go below 4 seconds (still less frequent than powerups)
    public int maxDifficultyLevel = 8; // Cap scaling at level 8

    private float spawnTimer = 0f;
    private float playerStartZ;
    private List<GameObject> activeCoins = new List<GameObject>();

    private void Start()
    {
        // Record starting position for distance calculation
        if (player != null)
        {
            playerStartZ = player.position.z;
        }
        
        spawnTimer = Random.Range(0, baseSpawnInterval);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver())
        {
            return;
        }

        activeCoins.RemoveAll(coin => coin == null);

        spawnTimer -= Time.deltaTime;
        
        float currentSpawnInterval = GetCurrentSpawnInterval();

        if (spawnTimer <= 0f && CanSpawnCoin())
        {
            SpawnCoin();
            spawnTimer = currentSpawnInterval + Random.Range(-spawnTimingVariability, spawnTimingVariability);
        }
    }

    private bool CanSpawnCoin()
    {
        return activeCoins.Count < maxCoinsOnScreen && coinPickupPrefab != null;
    }

    private float GetCurrentSpawnInterval()
    {
        if (!enableDifficultyScaling || player == null)
        {
            return baseSpawnInterval;
        }

        // Calculate distance traveled
        float distanceTraveled = player.position.z - playerStartZ;
        
        // Calculate difficulty level
        float difficultyLevel = distanceTraveled / difficultyIncreaseDistance;
        difficultyLevel = Mathf.Min(difficultyLevel, maxDifficultyLevel);
        
        // Calculate spawn interval reduction
        float intervalReduction = difficultyLevel * spawnRateIncreasePerLevel;
        float scaledInterval = baseSpawnInterval - intervalReduction;
        
        // Ensure we don't go below minimum
        scaledInterval = Mathf.Max(scaledInterval, minSpawnInterval);
        
        return scaledInterval;
    }

    private void SpawnCoin()
    {
        // Choose random lane
        float laneX = laneXPositions[Random.Range(0, laneXPositions.Length)];
        float spawnZ = player.position.z + 60f; // Spawn ahead of player
        Vector3 spawnPos = new Vector3(laneX, 1f, spawnZ);

        // Instantiate coin
        GameObject coin = Instantiate(coinPickupPrefab, spawnPos, Quaternion.identity);
        
        // Add to tracking list
        activeCoins.Add(coin);
        
        // Set up auto-destruction and cleanup
        StartCoroutine(CleanupCoinAfterTime(coin, coinLifetime));
        
        Debug.Log($"Coin spawned! Value: {coinsPerSpawn} coins | Active: {activeCoins.Count}/{maxCoinsOnScreen}");
    }

    private System.Collections.IEnumerator CleanupCoinAfterTime(GameObject coin, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        
        if (coin != null)
        {
            // Remove from tracking list before destroying
            activeCoins.Remove(coin);
            Destroy(coin);
            Debug.Log("Coin auto-destroyed (timeout)");
        }
    }

    // Called by CoinPickup when a coin is collected
    public void OnCoinCollected(GameObject coin)
    {
        if (activeCoins.Contains(coin))
        {
            activeCoins.Remove(coin);
            Debug.Log($"Coin collected! Remaining active: {activeCoins.Count}");
        }
    }

    // Public method to get current spawning info for debug
    public string GetSpawnInfo()
    {
        if (!enableDifficultyScaling || player == null)
        {
            return $"Coin Spawning: Static | Active: {activeCoins.Count}/{maxCoinsOnScreen}";
        }

        float distanceTraveled = player.position.z - playerStartZ;
        float difficultyLevel = Mathf.Min(distanceTraveled / difficultyIncreaseDistance, maxDifficultyLevel);
        float currentInterval = GetCurrentSpawnInterval();
        
        return $"Coin Spawn: Level {difficultyLevel:F1} | Interval: {currentInterval:F1}s | Active: {activeCoins.Count}/{maxCoinsOnScreen}";
    }

    // Public getter for coin value (used by CoinPickup)
    public int GetCoinValue()
    {
        return coinsPerSpawn;
    }
}