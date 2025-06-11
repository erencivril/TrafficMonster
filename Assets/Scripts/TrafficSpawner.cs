using System.Collections.Generic;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [Tooltip("Assign all your different traffic car prefabs here.")]
    public GameObject[] trafficCarPrefabs;
    
    [Header("Spawn Settings")]
    public Transform player; // Player reference for spawn distance calculation
    [SerializeField] private float baseSpawnInterval = 1.5f; // Reduced from 3f for more frequent spawning
    [SerializeField] private float spawnVariability = 0.8f; // Reduced variability for more consistent traffic
    [SerializeField] private int maxCarsPerSpawn = 2; // Actually use this to spawn multiple cars
    [SerializeField] private float spawnDistance = 150f;
    [Tooltip("The range of speeds for traffic cars. A value will be chosen randomly between X and Y.")]
    public Vector2 trafficSpeedRange = new Vector2(12f, 25f);

    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [SerializeField] private float difficultyIncreaseDistance = 1000f; // Every 1000 meters
    [SerializeField] private float spawnRateIncreasePerLevel = 0.2f;
    [SerializeField] private float maxDifficultyLevel = 15f;
    [SerializeField] private float minSpawnInterval = 0.4f; // Reduced from 0.8f for much more crowded traffic at high difficulty
    

    [Header("Pit Stop Integration")]
    [Tooltip("The zone (in meters) around the pit stop where traffic density is reduced.")]
    public float pitStopSafeZone = 60f;
    
    [Header("Layer Settings")]
    [Tooltip("The physics layer that the traffic cars are on.")]
    public LayerMask trafficLayer;

    [Header("Police Chase Integration")]
    [SerializeField] private float policeChaseTrafficReduction = 0.5f;

    private float timer;
    private float playerStartZ; // To track total distance traveled

    private void Start()
    {
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

        timer += Time.deltaTime;

        float currentSpawnInterval = GetCurrentSpawnInterval();

        if (timer >= currentSpawnInterval)
        {
            AttemptTrafficSpawn();
            
            // Reset timer with variability and police chase consideration
            float spawnInterval = currentSpawnInterval + Random.Range(-spawnVariability, spawnVariability);
            
            // Reduce traffic during police chases
            if (PoliceManager.Instance != null && PoliceManager.Instance.IsChasing())
            {
                spawnInterval *= (1f + policeChaseTrafficReduction); // Increase interval = reduce spawns
            }
            
            timer = 0f;
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
        difficultyLevel = Mathf.Min(difficultyLevel, maxDifficultyLevel); // Cap at max level
        
        // Calculate spawn interval reduction
        float intervalReduction = difficultyLevel * spawnRateIncreasePerLevel;
        float scaledInterval = baseSpawnInterval - intervalReduction;
        
        // Ensure we don't go below minimum
        scaledInterval = Mathf.Max(scaledInterval, minSpawnInterval);
        
        // Debug info (can be removed in final build)
        if (Time.frameCount % 300 == 0) // Log every 5 seconds at 60fps
        {
            Debug.Log($"Distance: {distanceTraveled:F0}m, Difficulty Level: {difficultyLevel:F1}, Spawn Interval: {scaledInterval:F1}s");
        }
        
        return scaledInterval;
    }

    private void AttemptTrafficSpawn()
    {
        // Basic safety checks
        if (trafficCarPrefabs.Length == 0)
        {
            Debug.LogError("No traffic car prefabs are assigned in the TrafficSpawner inspector!");
            return;
        }
        
        if (TrafficLaneManager.Instance == null)
        {
            Debug.LogError("TrafficLaneManager not found! Please add it to the scene.");
            return;
        }

        // Calculate base spawn position
        float baseSpawnZ = player.position.z + spawnDistance;
        float desiredSpeed = Random.Range(trafficSpeedRange.x, trafficSpeedRange.y);

        // Get available spawn opportunities from the lane manager
        List<int> availableLanes;
        List<float> maxSpeeds;
        TrafficLaneManager.Instance.GetSpawnOpportunities(baseSpawnZ, desiredSpeed, out availableLanes, out maxSpeeds);

        // Remove lanes that are in pit stop safe zone
        FilterPitStopLanes(ref availableLanes, ref maxSpeeds, baseSpawnZ);

        // If no lanes are available, skip this spawn cycle
        if (availableLanes.Count == 0)
        {
            Debug.Log("No safe lanes available for spawning at Z: " + baseSpawnZ);
            return;
        }

        // Spawn multiple cars per attempt for more crowded traffic
        int carsToSpawn = Random.Range(1, maxCarsPerSpawn + 1); // 1 to maxCarsPerSpawn cars
        int carsSpawned = 0;
        
        // Create a copy of available lanes so we can remove used ones
        List<int> remainingLanes = new List<int>(availableLanes);
        List<float> remainingSpeeds = new List<float>(maxSpeeds);
        
        for (int i = 0; i < carsToSpawn && remainingLanes.Count > 0; i++)
        {
            // Randomly select from remaining available lanes
            int randomIndex = Random.Range(0, remainingLanes.Count);
            int chosenLane = remainingLanes[randomIndex];
            float maxAllowedSpeed = remainingSpeeds[randomIndex];

        // Spawn the car
        SpawnTrafficCarInLane(chosenLane, baseSpawnZ, maxAllowedSpeed);
            carsSpawned++;
            
            // Remove this lane from remaining options to avoid spawning multiple cars in same lane
            remainingLanes.RemoveAt(randomIndex);
            remainingSpeeds.RemoveAt(randomIndex);
        }
        
        if (carsSpawned > 1)
        {
            Debug.Log($"Spawned {carsSpawned} cars in traffic burst at Z: {baseSpawnZ}");
        }
    }

    private void FilterPitStopLanes(ref List<int> availableLanes, ref List<float> maxSpeeds, float spawnZ)
    {
        if (!PitStopManager.Instance.IsPitStopActive())
        {
            return; // No active pit stop, no filtering needed
        }

        Vector3 pitStopPos = PitStopManager.Instance.GetCurrentPitStopPosition();
        float distanceToPitStop = Mathf.Abs(spawnZ - pitStopPos.z);

        if (distanceToPitStop < pitStopSafeZone)
        {
            int pitStopLane = PitStopManager.Instance.GetCurrentPitStopLaneIndex();
            
            // Remove pit stop lane from available options
            for (int i = availableLanes.Count - 1; i >= 0; i--)
            {
                if (availableLanes[i] == pitStopLane)
                {
                    availableLanes.RemoveAt(i);
                    maxSpeeds.RemoveAt(i);
                    Debug.Log("Filtered out pit stop lane " + pitStopLane + " from spawning options.");
                    break;
                }
            }
        }
    }

    private void SpawnTrafficCarInLane(int laneIndex, float spawnZ, float maxSpeed)
    {
        // Select a random car prefab
        GameObject prefabToSpawn = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];

        // Calculate spawn position
        float[] lanePositions = TrafficLaneManager.Instance.laneXPositions;
        Vector3 spawnPosition = new Vector3(lanePositions[laneIndex], 0f, spawnZ);

        // Instantiate the car
        GameObject car = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        // Set up the traffic car component
        TrafficCar trafficCarComponent = car.GetComponent<TrafficCar>();
        if (trafficCarComponent != null)
        {
            // Assign the safe speed (guaranteed not to cause collisions)
            float finalSpeed = Random.Range(trafficSpeedRange.x, maxSpeed);
            trafficCarComponent.Initialize(finalSpeed, laneIndex);
            
            Debug.Log($"Spawned {prefabToSpawn.name} in lane {laneIndex} at speed {finalSpeed:F1} (max allowed: {maxSpeed:F1})");
        }
        else
        {
            Debug.LogError("The prefab '" + prefabToSpawn.name + "' is missing the TrafficCar script!", car);
            Destroy(car);
        }
    }

    // Debug method to visualize traffic state
    [ContextMenu("Debug Traffic State")]
    public void DebugTrafficState()
    {
        if (TrafficLaneManager.Instance != null)
        {
            TrafficLaneManager.Instance.DebugLogTrafficState();
        }
    }

    // Public method to get current difficulty info for UI or other systems
    public string GetDifficultyInfo()
    {
        if (!enableDifficultyScaling || player == null)
        {
            return "Difficulty: Static";
        }

        float distanceTraveled = player.position.z - playerStartZ;
        float difficultyLevel = Mathf.Min(distanceTraveled / difficultyIncreaseDistance, maxDifficultyLevel);
        float currentInterval = GetCurrentSpawnInterval();
        
        return $"Distance: {distanceTraveled:F0}m | Level: {difficultyLevel:F1} | Spawn Rate: {currentInterval:F1}s";
    }
}