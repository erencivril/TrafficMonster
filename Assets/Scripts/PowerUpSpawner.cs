using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject speedBoostPrefab;
    public GameObject shieldCoinPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float baseSpawnInterval = 6f; // Reduced from 10f for more frequent spawning
    public float[] laneXPositions = { -3.3f, 0f, 3.3f };
    
    [Header("Difficulty Scaling")]
    public bool enableDifficultyScaling = true; // Scale with game difficulty
    public float difficultyIncreaseDistance = 1000f; // Same as other spawners
    public float spawnRateIncreasePerLevel = 0.5f; // Reduce interval by 0.5s per level
    public float minSpawnInterval = 2f; // Never go below 2 seconds

    private float spawnTimer = 0f;
    private int spawnCounter = 0; // Track spawns for shield frequency
    private float playerStartZ; // To track total distance traveled

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

        spawnTimer -= Time.deltaTime;
        
        float currentSpawnInterval = GetCurrentSpawnInterval();

        if (spawnTimer <= 0f)
        {
            SpawnPowerUp();
            spawnTimer = currentSpawnInterval;
        }
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
        difficultyLevel = Mathf.Min(difficultyLevel, 10f); // Cap at level 10
        
        // Calculate spawn interval reduction
        float intervalReduction = difficultyLevel * spawnRateIncreasePerLevel;
        float scaledInterval = baseSpawnInterval - intervalReduction;
        
        // Ensure we don't go below minimum
        scaledInterval = Mathf.Max(scaledInterval, minSpawnInterval);
        
        return scaledInterval;
    }

    private void SpawnPowerUp()
    {
        float laneX = laneXPositions[Random.Range(0, laneXPositions.Length)];
        float spawnZ = player.position.z + 60f;
        Vector3 spawnPos = new Vector3(laneX, 1f, spawnZ);

        spawnCounter++;
        
        // Shield spawns every 3rd power-up (33% chance instead of 50%)
        bool shouldSpawnShield = (spawnCounter % 3 == 0);

        if (shouldSpawnShield)
        {
            GameObject shield = Instantiate(shieldCoinPrefab, spawnPos, Quaternion.identity);
            Destroy(shield, 15f); // Increased lifetime since spawning more frequently
            Debug.Log($"Shield spawned! (Spawn #{spawnCounter})");
        }
        else
        {
            GameObject speedBoost = Instantiate(speedBoostPrefab, spawnPos, Quaternion.identity);
            Destroy(speedBoost, 15f); // Increased lifetime since spawning more frequently
            Debug.Log($"Speed Boost spawned! (Spawn #{spawnCounter})");
        }
    }

    // Public method to get current spawning info for debug
    public string GetSpawnInfo()
    {
        if (!enableDifficultyScaling || player == null)
        {
            return "Power-up Spawning: Static";
        }

        float distanceTraveled = player.position.z - playerStartZ;
        float difficultyLevel = Mathf.Min(distanceTraveled / difficultyIncreaseDistance, 10f);
        float currentInterval = GetCurrentSpawnInterval();
        
        return $"Power-up Spawn: Level {difficultyLevel:F1} | Interval: {currentInterval:F1}s";
    }
} 