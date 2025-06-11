using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    // --- Current Upgrade Levels (Session-Based Only) ---
    // These reset every game session - no persistence
    public int EngineLevel { get; private set; } = 1;
    public int FuelTankLevel { get; private set; } = 1;
    public int HandlingLevel { get; private set; } = 1;

    // --- Upgrade Stat Values ---
    // These arrays hold the actual stat values for each upgrade level.
    // Level 1 (index 0) is the base value.
    [Header("Engine Stats (Max Speed) - Session-Based")]
    public float[] engineSpeedLevels = { 30f, 40f, 52f, 66f, 82f }; // Significant increases for session impact

    [Header("Fuel Tank Stats (Max Fuel) - Session-Based")]
    public float[] fuelTankCapacityLevels = { 60f, 75f, 92f, 112f, 135f }; // Good progression for session play

    [Header("Handling Stats (Lane Change) - Session-Based")]
    public float[] laneChangeSpeedLevels = { 15f, 18f, 21f, 24f, 27f };
    [Tooltip("The percentage of speed KEPT after a lane change. 0.8 = 20% speed loss. 1.0 = 0% speed loss.")]
    public float[] handlingSpeedRetainLevels = { 0.5f, 0.65f, 0.8f, 0.9f, 1.0f }; // More noticeable progression: 50%→35%→20%→10%→0% speed loss

    [Header("Upgrade Costs (Session-Based Economy)")]
    [Tooltip("Cost for each engine upgrade level")]
    public int[] engineUpgradeCosts = { 200, 450, 750, 1200 }; // Rebalanced for strategic upgrade choices
    [Tooltip("Cost for each fuel tank upgrade level")]
    public int[] fuelTankUpgradeCosts = { 150, 350, 600, 950 };
    [Tooltip("Cost for each handling upgrade level")]
    public int[] handlingUpgradeCosts = { 180, 400, 650, 1000 };

    private void Awake()
    {
        // Singleton pattern - NO persistence across scenes
        if (Instance == null)
        {
            Instance = this;
            // NOTE: No DontDestroyOnLoad - upgrades reset when restarting game
        }
        else
        {
            Destroy(gameObject);
        }

        // Start fresh every game session
        ResetUpgrades();
    }

    // --- Public Methods to Get Current Stats ---
    public float GetCurrentMaxSpeed()
    {
        return engineSpeedLevels[EngineLevel - 1];
    }

    public float GetCurrentMaxFuel()
    {
        return fuelTankCapacityLevels[FuelTankLevel - 1];
    }

    public float GetCurrentHandling()
    {
        return laneChangeSpeedLevels[HandlingLevel - 1];
    }

    public float GetCurrentHandlingPenalty()
    {
        return handlingSpeedRetainLevels[HandlingLevel - 1];
    }

    // --- Upgrade Cost Methods ---
    public int GetEngineUpgradeCost()
    {
        if (EngineLevel >= engineSpeedLevels.Length) return -1; // Max level
        return engineUpgradeCosts[EngineLevel - 1];
    }

    public int GetFuelTankUpgradeCost()
    {
        if (FuelTankLevel >= fuelTankCapacityLevels.Length) return -1; // Max level
        return fuelTankUpgradeCosts[FuelTankLevel - 1];
    }

    public int GetHandlingUpgradeCost()
    {
        if (HandlingLevel >= laneChangeSpeedLevels.Length) return -1; // Max level
        return handlingUpgradeCosts[HandlingLevel - 1];
    }

    // --- Public Methods to Purchase Upgrades ---
    public bool PurchaseEngineUpgrade()
    {
        if (EngineLevel >= engineSpeedLevels.Length) return false; // Max level

        int cost = GetEngineUpgradeCost();
        if (GameManager.Instance.CanAfford(cost))
        {
            GameManager.Instance.SpendCoins(cost);
            EngineLevel++;
            Debug.Log($"Engine upgraded to level {EngineLevel} - New max speed: {GetCurrentMaxSpeed()} - Cost: {cost}");
            
            // Refresh CarController with new values
            RefreshCarController();
            
            // Notify police system that player got stronger
            if (PoliceManager.Instance != null)
            {
                PoliceManager.Instance.OnPlayerUpgrade();
            }
            return true;
        }
        return false;
    }

    public bool PurchaseFuelTankUpgrade()
    {
        if (FuelTankLevel >= fuelTankCapacityLevels.Length) return false; // Max level

        int cost = GetFuelTankUpgradeCost();
        if (GameManager.Instance.CanAfford(cost))
        {
            GameManager.Instance.SpendCoins(cost);
            FuelTankLevel++;
            Debug.Log($"Fuel Tank upgraded to level {FuelTankLevel} - New capacity: {GetCurrentMaxFuel()} - Cost: {cost}");
            
            // Refresh CarController with new values (though fuel doesn't affect CarController directly)
            RefreshCarController();
            return true;
        }
        return false;
    }

    public bool PurchaseHandlingUpgrade()
    {
        if (HandlingLevel >= laneChangeSpeedLevels.Length) return false; // Max level

        int cost = GetHandlingUpgradeCost();
        if (GameManager.Instance.CanAfford(cost))
        {
            GameManager.Instance.SpendCoins(cost);
            HandlingLevel++;
            Debug.Log($"Handling upgraded to level {HandlingLevel} - Lane change penalty: {(1f - GetCurrentHandlingPenalty()) * 100f:F0}% - Cost: {cost}");
            
            // Refresh CarController with new values
            RefreshCarController();
            
            // Notify police system that player got stronger
            if (PoliceManager.Instance != null)
            {
                PoliceManager.Instance.OnPlayerUpgrade();
            }
            return true;
        }
        return false;
    }

    // --- Reset Method (Called on Game Start) ---
    public void ResetUpgrades()
    {
        EngineLevel = 1;
        FuelTankLevel = 1;
        HandlingLevel = 1;
        Debug.Log("All upgrades reset for new game session.");
    }

    // Public method to get total upgrade level (for police scaling)
    public int GetTotalUpgradeLevel()
    {
        return (EngineLevel - 1) + (FuelTankLevel - 1) + (HandlingLevel - 1);
    }

    // Public method to get upgrade info for UI
    public string GetUpgradeInfo()
    {
        return $"Engine L{EngineLevel} ({GetCurrentMaxSpeed():F0} mph) | " +
               $"Fuel L{FuelTankLevel} ({GetCurrentMaxFuel():F0} units) | " +
               $"Handling L{HandlingLevel} ({GetCurrentHandlingPenalty() * 100f:F0}% speed kept)";
    }

    // Public method to check if any upgrades are available
    public bool HasAvailableUpgrades(int playerCoins)
    {
        return (EngineLevel < engineSpeedLevels.Length && playerCoins >= GetEngineUpgradeCost()) ||
               (FuelTankLevel < fuelTankCapacityLevels.Length && playerCoins >= GetFuelTankUpgradeCost()) ||
               (HandlingLevel < laneChangeSpeedLevels.Length && playerCoins >= GetHandlingUpgradeCost());
    }

    // Method to refresh CarController values after upgrades
    private void RefreshCarController()
    {
        CarController carController = FindObjectOfType<CarController>();
        if (carController != null)
        {
            carController.RefreshUpgradeValues();
        }
        
        // Also refresh FuelSystem for fuel tank upgrades
        FuelSystem fuelSystem = FindObjectOfType<FuelSystem>();
        if (fuelSystem != null)
        {
            fuelSystem.RefreshUpgradeValues();
        }
    }
} 