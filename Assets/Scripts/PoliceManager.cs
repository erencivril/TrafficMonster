using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PoliceManager : MonoBehaviour
{
    public static PoliceManager Instance;

    [Header("Chase Setup")]
    public GameObject policeCarPrefab;
    public Transform player;

    [Header("Heat System")]
    public float maxHeat = 100f;
    public float heatIncreaseRate = 7f; // Points per second during normal driving
        
    [Header("Police Chase Settings")]
    public float basePoliceSpeedAdvantage = 3f; // Increased for more aggressive chase
    public float timeBasedSpeedIncrease = 0.1f; // Speed increase per minute of gameplay
    public float upgradeSpeedBonus = 0.3f; // Reduced from 0.4f - additional speed per upgrade level
    public float chaseStartDistance = 25f; // Police spawn closer to player for immediate pressure
    public float despawnDistance = 200f; // How far ahead player must get to despawn police (emergency escape)
    
    [Header("Police Scaling")]
    public bool enablePoliceScaling = true; // Toggle police scaling
    public float maxPoliceSpeedAdvantage = 6f; // Reduced from 8f for session-based balance
    public int upgradesForSpeedIncrease = 1; // Every upgrade increases police speed (more responsive)

    [Header("UI")]
    public TextMeshProUGUI heatText;
    public Slider bustSlider;

    private float currentHeat = 0f;
    private bool isChasing = false;
    private GameObject activePoliceCar;
    private int lastKnownUpgradeLevel = 0; // Track upgrade progression
    private float gameStartTime; // Track time for time-based scaling

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        if (heatText != null)
            heatText.gameObject.SetActive(true);
        
        if (bustSlider != null)
            bustSlider.gameObject.SetActive(false);

        // Initialize upgrade tracking
        if (UpgradeManager.Instance != null)
        {
            lastKnownUpgradeLevel = UpgradeManager.Instance.GetTotalUpgradeLevel();
        }

        gameStartTime = Time.time;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver()) return;

        HandleHeatSystem();
        HandleChaseLogic();
        UpdateUI();
    }

    private void HandleHeatSystem()
    {
        // Only increase heat if not chasing (during chase, heat stays constant)
        if (!isChasing)
        {
            currentHeat += heatIncreaseRate * Time.deltaTime;
            currentHeat = Mathf.Min(currentHeat, maxHeat);
            
            // Start chase when heat reaches maximum
            if (currentHeat >= maxHeat && !isChasing)
            {
                StartChase();
            }
        }
    }

    private void HandleChaseLogic()
    {
        if (isChasing && activePoliceCar != null)
        {
            // Check for emergency escape (player got very far ahead)
            float distanceToPlayer = player.position.z - activePoliceCar.transform.position.z;
            if (distanceToPlayer > despawnDistance)
            {
                Debug.Log("Player escaped police by distance!");
                EndChase(false); // false = no heat reset, emergency escape
            }
            
            // Update bust meter
            if (bustSlider != null)
            {
                float bustProgress = activePoliceCar.GetComponent<PoliceAI>().BustProgress;
                bustSlider.value = bustProgress;
            }
        }
    }

    private float GetCurrentPoliceSpeedAdvantage()
    {
        if (!enablePoliceScaling)
        {
            return basePoliceSpeedAdvantage;
        }

        float totalAdvantage = basePoliceSpeedAdvantage;

        // Add time-based scaling (police get better over time)
        float gameTimeMinutes = (Time.time - gameStartTime) / 60f;
        totalAdvantage += gameTimeMinutes * timeBasedSpeedIncrease;

        // Add upgrade-based scaling (police respond to player getting stronger)
        if (UpgradeManager.Instance != null)
        {
            int totalUpgrades = UpgradeManager.Instance.GetTotalUpgradeLevel();
            int speedIncreases = totalUpgrades / upgradesForSpeedIncrease;
            totalAdvantage += speedIncreases * upgradeSpeedBonus;
        }
        
        // Cap the advantage to prevent impossibility
        totalAdvantage = Mathf.Min(totalAdvantage, maxPoliceSpeedAdvantage);
        
        return totalAdvantage;
    }

    private void ResetHeatAndDespawnPolice()
    {
        currentHeat = 0f;
        EndChase(true); // true = heat was reset via pit stop
    }

    private void StartChase()
    {
        if (isChasing) return; // Already chasing
        
        isChasing = true;
        
        // Calculate current police speed advantage
        float currentSpeedAdvantage = GetCurrentPoliceSpeedAdvantage();
        Debug.Log($"Police chase started! Speed advantage: {currentSpeedAdvantage:F1}");

        // Show the bust slider
        if (bustSlider != null)
            bustSlider.gameObject.SetActive(true);

        // Spawn police car behind the player
        Vector3 spawnPos = player.position - new Vector3(0, 0, chaseStartDistance);
        
        // Spawn in center lane to be fair
        spawnPos.x = 0f;
        
        activePoliceCar = Instantiate(policeCarPrefab, spawnPos, player.rotation);
        
        // Initialize the AI with scaled speed
        PoliceAI policeAI = activePoliceCar.GetComponent<PoliceAI>();
        if (policeAI != null)
        {
            // Get player's current max speed and add scaled advantage
            float playerMaxSpeed = UpgradeManager.Instance.GetCurrentMaxSpeed();
            float policeSpeed = playerMaxSpeed + currentSpeedAdvantage;
            policeAI.Initialize(player, policeSpeed);
            
            Debug.Log($"Police speed: {policeSpeed:F1} (Player: {playerMaxSpeed:F1} + Advantage: {currentSpeedAdvantage:F1})");
        }

       
    }

    private void EndChase(bool heatReset)
    {
        if (!isChasing) return;
        
        isChasing = false;
        
        string reason = heatReset ? "reached pit stop" : "escaped by distance";
        Debug.Log($"Police chase ended: player {reason}");

        // Hide the bust slider
        if (bustSlider != null)
            bustSlider.gameObject.SetActive(false);

        if (activePoliceCar != null)
        {

            Destroy(activePoliceCar);
            activePoliceCar = null;
        }
    }

    private void UpdateUI()
    {
        // Update heat text
        if (heatText != null)
        {
            if (isChasing)
            {
                heatText.text = "CHASED!";
                heatText.color = Color.red;
            }
            else
            {
                heatText.text = "Heat: " + (int)currentHeat + "%";
                heatText.color = Color.white;
            }
        }
    }

    // Public method for other systems to check chase state
    public bool IsChasing()
    {
        return isChasing;
    }

    // Public method to get current heat level
    public float GetCurrentHeat()
    {
        return currentHeat;
    }

    // Public method for pit stops to call when player uses continue button
    public void PitStopEscape()
    {
        Debug.Log("Player used pit stop! Heat reset and police despawned.");
        ResetHeatAndDespawnPolice();
    }

    // Public method called when player purchases upgrades
    public void OnPlayerUpgrade()
    {
        if (UpgradeManager.Instance == null) return;

        int newUpgradeLevel = UpgradeManager.Instance.GetTotalUpgradeLevel();
        
        if (newUpgradeLevel > lastKnownUpgradeLevel)
        {
            float newSpeedAdvantage = GetCurrentPoliceSpeedAdvantage();
            Debug.Log($"Player upgraded! Police will now have {newSpeedAdvantage:F1} speed advantage in next chase.");
            lastKnownUpgradeLevel = newUpgradeLevel;
        }
    }

    // Public method to get police scaling info for debug/UI
    public string GetPoliceScalingInfo()
    {
        if (!enablePoliceScaling || UpgradeManager.Instance == null)
        {
            return "Police Scaling: Disabled";
        }

        int totalUpgrades = UpgradeManager.Instance.GetTotalUpgradeLevel();
        float currentAdvantage = GetCurrentPoliceSpeedAdvantage();
        
        return $"Police Scaling: Level {totalUpgrades} | Speed Advantage: {currentAdvantage:F1}";
    }
} 