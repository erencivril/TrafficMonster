using UnityEngine;
using UnityEngine.UI;
using TMPro;

// An enum to define the different types of upgrades available.
public enum UpgradeType
{
    Engine,
    FuelTank,
    Handling
}

public class UpgradeButton : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public UpgradeType upgradeType; // Set this in the Inspector for each button

    // We will get references to these components automatically
    [HideInInspector] public Button button;
    [HideInInspector] public TextMeshProUGUI costText;
    [HideInInspector] public TextMeshProUGUI levelText;

    private void Awake()
    {
        // Get the components from the button's children
        button = GetComponent<Button>();
        costText = transform.Find("CostText").GetComponent<TextMeshProUGUI>(); // Assumes a child named "CostText"
        levelText = transform.Find("LevelText").GetComponent<TextMeshProUGUI>(); // Assumes a child named "LevelText"
    }
} 