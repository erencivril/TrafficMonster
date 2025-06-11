using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject shopPanel;
    public Button continueButton;
    public TextMeshProUGUI playerCoinsText;

    // An array of all the upgrade buttons in the shop
    public UpgradeButton[] upgradeButtons;

    void Start()
    {
        // Add a listener to the continue button to close the shop
        continueButton.onClick.AddListener(CloseShop);

        // Add listeners to each upgrade button
        foreach (var ub in upgradeButtons)
        {
            UpgradeButton buttonInstance = ub; 
            ub.button.onClick.AddListener(() => OnUpgradeButtonPressed(buttonInstance));
        }
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        UpdateUI();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        
        // Resume the game
        Time.timeScale = 1f;
        
        // Reset police heat and despawn police when player uses pit stop
        if (PoliceManager.Instance != null)
        {
            PoliceManager.Instance.PitStopEscape();
        }
        
        // Tell the PitStopManager to spawn the next pit stop
        PitStopManager.Instance.PitStopEscape();
    }

    private void OnUpgradeButtonPressed(UpgradeButton button)
    {
        // Try to purchase the upgrade directly from UpgradeManager
        bool success = false;
        
        switch (button.upgradeType)
        {
            case UpgradeType.Engine:
                success = UpgradeManager.Instance.PurchaseEngineUpgrade();
                break;
            case UpgradeType.FuelTank:
                success = UpgradeManager.Instance.PurchaseFuelTankUpgrade();
                break;
            case UpgradeType.Handling:
                success = UpgradeManager.Instance.PurchaseHandlingUpgrade();
                break;
        }

        if (success)
        {
            // If purchase was successful, update the entire UI to reflect new levels and costs
            UpdateUI();
        }
        else
        {
            // Optional: Add some feedback for a failed purchase (e.g., a sound effect)
            Debug.Log("Purchase failed. Not enough coins or max level reached.");
        }
    }

    // This method updates all the text and button states in the shop
    private void UpdateUI()
    {
        // Update player's coin display
        playerCoinsText.text = "Your Coins: " + (int)GameManager.Instance.Coins;

        // Loop through each upgrade button and update its display
        foreach (var ub in upgradeButtons)
        {
            int currentLevel;
            int cost;
            int maxLevel;
            string levelText;

            switch (ub.upgradeType)
            {
                case UpgradeType.Engine:
                    currentLevel = UpgradeManager.Instance.EngineLevel;
                    maxLevel = UpgradeManager.Instance.engineSpeedLevels.Length;
                    cost = (currentLevel < maxLevel) ? UpgradeManager.Instance.GetEngineUpgradeCost() : 0;
                    levelText = "Engine Lvl " + currentLevel;
                    break;
                case UpgradeType.FuelTank:
                    currentLevel = UpgradeManager.Instance.FuelTankLevel;
                    maxLevel = UpgradeManager.Instance.fuelTankCapacityLevels.Length;
                    cost = (currentLevel < maxLevel) ? UpgradeManager.Instance.GetFuelTankUpgradeCost() : 0;
                    levelText = "Fuel Tank Lvl " + currentLevel;
                    break;
                case UpgradeType.Handling:
                    currentLevel = UpgradeManager.Instance.HandlingLevel;
                    maxLevel = UpgradeManager.Instance.laneChangeSpeedLevels.Length;
                    cost = (currentLevel < maxLevel) ? UpgradeManager.Instance.GetHandlingUpgradeCost() : 0;
                    levelText = "Handling Lvl " + currentLevel;
                    break;
                default:
                    return; // Should not happen
            }

            // Update the button's text
            ub.levelText.text = levelText;

            // Check if max level is reached
            if (currentLevel >= maxLevel)
            {
                ub.button.interactable = false;
                ub.costText.text = "MAX";
            }
            else
            {
                // Update cost text and enable/disable button based on affordability
                ub.costText.text = "Cost: " + cost;
                ub.button.interactable = GameManager.Instance.CanAfford(cost);
            }
        }
    }
} 