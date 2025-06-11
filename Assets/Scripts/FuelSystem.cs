using UnityEngine;
using TMPro;

public class FuelSystem : MonoBehaviour
{
    [Header("Fuel Settings")]
    public float maxFuel = 60f; 
    public float currentFuel;
    public float baseFuelConsumptionRate = 2f; 
    public float speedConsumptionMultiplier = 0.02f; 
    public float laneChangeFuelCost = 1f; 
    
    [Header("UI")]
    public TextMeshProUGUI fuelText; 

    private CarController carController;
    private bool wasChangingLanes = false;

    private void Start()
    {
        // Get car controller reference
        carController = FindObjectOfType<CarController>();
        
        // --- Apply Upgrades from UpgradeManager ---
        if (UpgradeManager.Instance != null)
        {
            maxFuel = UpgradeManager.Instance.GetCurrentMaxFuel();
        }

        currentFuel = maxFuel;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver())
        {
            return;
        }

        // Calculate fuel consumption based on speed
        float currentSpeed = carController != null ? Mathf.Abs(carController.MoveSpeed) : 0f;
        float speedBasedConsumption = currentSpeed * speedConsumptionMultiplier;
        float totalConsumption = baseFuelConsumptionRate + speedBasedConsumption;

        // Consume fuel
        currentFuel -= totalConsumption * Time.deltaTime;
        currentFuel = Mathf.Clamp(currentFuel, 0, maxFuel);

        // Check for lane changes and consume extra fuel
        HandleLaneChangeFuelCost();

        UpdateFuelUI();

        if (currentFuel <= 0)
        {
            GameManager.Instance.ShowGameOver();
        }
    }

    private void HandleLaneChangeFuelCost()
    {
        if (carController == null) return;

        // Detect lane change by checking if we're currently changing lanes
        bool isChangingLanes = IsCarChangingLanes();
        
        // If we just started changing lanes (not changing before, but changing now)
        if (isChangingLanes && !wasChangingLanes)
        {
            // Consume fuel for lane change
            currentFuel -= laneChangeFuelCost;
            currentFuel = Mathf.Max(currentFuel, 0f);
            
            Debug.Log($"Lane change! Fuel cost: {laneChangeFuelCost}, Remaining fuel: {currentFuel:F1}");
        }
        
        wasChangingLanes = isChangingLanes;
    }

    private bool IsCarChangingLanes()
    {
        if (carController == null) return false;
        
        // Check if car's X position is not aligned with any lane (meaning it's between lanes)
        float[] lanePositions = { -3.3f, 0f, 3.3f };
        float currentX = carController.transform.position.x;
        
        foreach (float laneX in lanePositions)
        {
            if (Mathf.Abs(currentX - laneX) < 0.2f) // Close enough to a lane
            {
                return false;
            }
        }
        
        return true; // Not close to any lane, must be changing
    }

    public void AddFuel(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Min(currentFuel, maxFuel); // Don't exceed max fuel
        
        Debug.Log($"Fuel added: {amount}, Current fuel: {currentFuel:F1}");
    }

    private void UpdateFuelUI()
    {
        if (fuelText != null)
        {
            // Show fuel with decimal for better precision at low values
            if (currentFuel < 10f)
            {
                fuelText.text = "Fuel: " + currentFuel.ToString("F1");
                // Change color to red when fuel is low
                fuelText.color = currentFuel < 5f ? Color.red : Color.yellow;
            }
            else
            {
                fuelText.text = "Fuel: " + Mathf.CeilToInt(currentFuel);
                fuelText.color = Color.white;
            }
        }
    }

    // Public method to get current fuel percentage for other systems
    public float GetFuelPercentage()
    {
        return currentFuel / maxFuel;
    }

    // Public method to check if fuel is critically low
    public bool IsFuelCritical()
    {
        return currentFuel < 10f;
    }

    // Method to refresh upgrade values from UpgradeManager
    public void RefreshUpgradeValues()
    {
        if (UpgradeManager.Instance != null)
        {
            float oldMaxFuel = maxFuel;
            maxFuel = UpgradeManager.Instance.GetCurrentMaxFuel();
            
            // If max fuel increased, add the difference to current fuel (like getting a free refill)
            if (maxFuel > oldMaxFuel)
            {
                float difference = maxFuel - oldMaxFuel;
                currentFuel += difference;
                currentFuel = Mathf.Min(currentFuel, maxFuel);
                Debug.Log($"FuelSystem: Max fuel upgraded from {oldMaxFuel} to {maxFuel}. Added {difference} fuel.");
            }
            
            Debug.Log($"FuelSystem: Refreshed upgrade values - Max Fuel: {maxFuel}");
        }
    }
} 