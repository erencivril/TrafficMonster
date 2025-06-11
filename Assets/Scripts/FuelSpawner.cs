using UnityEngine;

public class FuelSpawner : MonoBehaviour
{
    public GameObject fuelPickupPrefab;
    public Transform player;
    
    [Header("Spawn Settings")]
    public float baseSpawnInterval = 3f; 
    public float spawnDistance = 80f;
    public float[] laneXPositions = { -3.3f, 0f, 3.3f };
    
    [Header("Dynamic Spawning")]
    public bool adaptiveSpawning = true; // Spawn more fuel when player fuel is low
    public float lowFuelThreshold = 0.3f; // 30% fuel triggers more frequent spawning
    public float lowFuelSpawnMultiplier = 0.4f; // 60% faster spawning when fuel is low
    
    [Header("Difficulty Scaling")]
    public bool enableDifficultyScaling = true; // Scale with game difficulty
    public float difficultyIncreaseDistance = 1000f; 
    public float spawnRateIncreasePerLevel = 0.4f;
    public float minSpawnInterval = 2f; // Never go below 2 seconds

    private float spawnTimer;
    private FuelSystem playerFuelSystem;
    private float playerStartZ; // To track total distance traveled

    private void Start()
    {
        // Start with a random delay to not have all spawners fire at once
        spawnTimer = Random.Range(0, baseSpawnInterval);
        
        // Get reference to player's fuel system
        playerFuelSystem = FindObjectOfType<FuelSystem>();
        
        // Record starting position for distance calculation
        if (player != null)
        {
            playerStartZ = player.position.z;
        }
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
            SpawnFuel();
            spawnTimer = currentSpawnInterval;
        }
    }

    private float GetCurrentSpawnInterval()
    {
        float interval = baseSpawnInterval;
        
        // Apply difficulty scaling
        if (enableDifficultyScaling && player != null)
        {
            float distanceTraveled = player.position.z - playerStartZ;
            float difficultyLevel = distanceTraveled / difficultyIncreaseDistance;
            difficultyLevel = Mathf.Min(difficultyLevel, 10f); // Cap at level 10
            
            float intervalReduction = difficultyLevel * spawnRateIncreasePerLevel;
            interval = baseSpawnInterval - intervalReduction;
            interval = Mathf.Max(interval, minSpawnInterval);
        }
        
        // Apply adaptive spawning for low fuel
        if (adaptiveSpawning && playerFuelSystem != null)
        {
            float fuelPercentage = playerFuelSystem.GetFuelPercentage();
            
            if (fuelPercentage <= lowFuelThreshold)
            {
                // Spawn fuel more frequently when fuel is low
                interval *= lowFuelSpawnMultiplier;
            }
        }

        return interval;
    }

    void SpawnFuel()
    {
        // Pick a random lane
        float laneX = laneXPositions[Random.Range(0, laneXPositions.Length)];
        
        // Calculate spawn position ahead of the player
        float spawnZ = player.position.z + spawnDistance;
        Vector3 spawnPos = new Vector3(laneX, 1f, spawnZ);

        // Instantiate the fuel pickup and set it to be destroyed after some time
        GameObject fuel = Instantiate(fuelPickupPrefab, spawnPos, Quaternion.identity);
        Destroy(fuel, 30f); 
        
        // Debug info
        if (playerFuelSystem != null && playerFuelSystem.IsFuelCritical())
        {
            Debug.Log($"Emergency fuel spawned! Player fuel: {playerFuelSystem.GetFuelPercentage() * 100f:F1}%");
        }
    }

    // Public method to get current spawning info for debug
    public string GetSpawnInfo()
    {
        if (!enableDifficultyScaling || player == null)
        {
            return "Fuel Spawning: Static";
        }

        float distanceTraveled = player.position.z - playerStartZ;
        float difficultyLevel = Mathf.Min(distanceTraveled / difficultyIncreaseDistance, 10f);
        float currentInterval = GetCurrentSpawnInterval();
        
        return $"Fuel Spawn: Level {difficultyLevel:F1} | Interval: {currentInterval:F1}s";
    }
} 