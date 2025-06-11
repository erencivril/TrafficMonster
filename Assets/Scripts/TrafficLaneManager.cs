using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LaneTraffic
{
    public List<TrafficCar> carsInLane = new List<TrafficCar>();
    
    // Get the frontmost (highest Z) car in this lane
    public TrafficCar GetFrontmostCar()
    {
        if (carsInLane.Count == 0) return null;
        
        TrafficCar frontmost = carsInLane[0];
        foreach (TrafficCar car in carsInLane)
        {
            if (car != null && car.transform.position.z > frontmost.transform.position.z)
            {
                frontmost = car;
            }
        }
        return frontmost;
    }
    
    // Clean up null references (destroyed cars)
    public void CleanupDestroyedCars()
    {
        carsInLane.RemoveAll(car => car == null);
    }
}

public class TrafficLaneManager : MonoBehaviour
{
    public static TrafficLaneManager Instance;
    
    [Header("Lane Configuration")]
    public float[] laneXPositions = { -3.3f, 0f, 3.3f };
    
    [Header("Safety Settings")]
    [Tooltip("Minimum distance between cars regardless of speed")]
    public float minimumGap = 8f;
    [Tooltip("Additional safety distance per unit of speed difference")]
    public float safetyDistancePerSpeed = 0.5f;
    [Tooltip("Speed buffer to prevent new cars from catching up")]
    public float speedBuffer = 2f;
    
    private LaneTraffic[] lanes;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeLanes();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeLanes()
    {
        lanes = new LaneTraffic[laneXPositions.Length];
        for (int i = 0; i < lanes.Length; i++)
        {
            lanes[i] = new LaneTraffic();
        }
    }
    
    // Register a car in its lane
    public void RegisterCar(TrafficCar car, int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < lanes.Length)
        {
            lanes[laneIndex].carsInLane.Add(car);
        }
    }
    
    // Unregister a car from its lane
    public void UnregisterCar(TrafficCar car, int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < lanes.Length)
        {
            lanes[laneIndex].carsInLane.Remove(car);
        }
    }
    
    // Get which lane index a car is in based on its X position
    public int GetLaneIndex(float xPosition)
    {
        int closestLane = 0;
        float closestDistance = Mathf.Abs(xPosition - laneXPositions[0]);
        
        for (int i = 1; i < laneXPositions.Length; i++)
        {
            float distance = Mathf.Abs(xPosition - laneXPositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = i;
            }
        }
        
        return closestLane;
    }
    
    // Check if it's safe to spawn a car in a specific lane
    public bool CanSpawnInLane(int laneIndex, float spawnZ, float proposedSpeed, out float maxAllowedSpeed)
    {
        maxAllowedSpeed = proposedSpeed;
        
        if (laneIndex < 0 || laneIndex >= lanes.Length)
        {
            return false;
        }
        
        // Clean up destroyed cars first
        lanes[laneIndex].CleanupDestroyedCars();
        
        TrafficCar frontmostCar = lanes[laneIndex].GetFrontmostCar();
        
        // If no cars in lane, spawn freely
        if (frontmostCar == null)
        {
            return true;
        }
        
        // Calculate required safe distance
        float speedDifference = Mathf.Max(0, proposedSpeed - frontmostCar.MoveSpeed);
        float requiredDistance = minimumGap + (speedDifference * safetyDistancePerSpeed);
        float actualDistance = frontmostCar.transform.position.z - spawnZ;
        
        // Check if we have enough space
        if (actualDistance < requiredDistance)
        {
            return false; // Not enough space
        }
        
        // Cap the speed to prevent catching up
        maxAllowedSpeed = Mathf.Min(proposedSpeed, frontmostCar.MoveSpeed - speedBuffer);
        
        return maxAllowedSpeed > 0; // Only spawn if we can have positive speed
    }
    
    // Get safe spawn information for all lanes
    public void GetSpawnOpportunities(float baseSpawnZ, float proposedSpeed, out List<int> availableLanes, out List<float> maxSpeeds)
    {
        availableLanes = new List<int>();
        maxSpeeds = new List<float>();
        
        for (int i = 0; i < lanes.Length; i++)
        {
            float maxSpeed;
            if (CanSpawnInLane(i, baseSpawnZ, proposedSpeed, out maxSpeed))
            {
                availableLanes.Add(i);
                maxSpeeds.Add(maxSpeed);
            }
        }
    }
    
    // Debug method to visualize traffic state
    public void DebugLogTrafficState()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            Debug.Log($"Lane {i}: {lanes[i].carsInLane.Count} cars");
        }
    }
} 