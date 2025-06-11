using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public ShopUI shopUI;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI speedMeterText;

    public TextMeshProUGUI shieldTimerText;
    public TextMeshProUGUI distanceToGoalText;
    public TextMeshProUGUI victoryTimeText; // Display completion time on victory screen

    [Header("Player & Camera")]
    public Transform player;
    public CameraShake cameraShake;
    private CarController carController;

    [Header("Audio")]
    public AudioSource crashSource;
    public AudioClip crashClip;

    [Header("Shield Settings")]
    public GameObject shieldVisualPrefab;

    [Header("Speed Boost Settings")]
    public TextMeshProUGUI speedBoostTimerText;

    [Header("Game Goal Settings")]
    public float totalJourneyDistance = 5000f; 
    private float totalProgress = 0f;
    private float gameStartTime;
    
    [Header("Session-Based Economy")]
    public float coinMultiplier = 0.2f; 

    public float Coins { get; private set; } = 0f;
    private float startZ; 
    private float lastCoinZ; 



    private bool isGameOver = false;

    private GameObject activeShield;
    private bool shieldActive = false;
    private float shieldTimer = 0f;

    // Speed boost variables
    private bool speedBoostActive = false;
    private float speedBoostTimer = 0f;
    private float speedBoostMultiplier = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);



        if (shieldTimerText != null)
            shieldTimerText.gameObject.SetActive(false);
        
        if (speedBoostTimerText != null)
            speedBoostTimerText.gameObject.SetActive(false);
        
        totalProgress = 0f;
        gameStartTime = Time.time;
    }

    private void Start()
    {
        startZ = player.position.z;
        lastCoinZ = player.position.z; 
        carController = FindObjectOfType<CarController>();
        Debug.Log($"Game started at position Z: {startZ}, Goal distance: {totalJourneyDistance}");
    }

    private void Update()
    {
        if (isGameOver) return;

      
        float totalDistanceTraveled = player.position.z - startZ;
        float distanceRemaining = totalJourneyDistance - totalDistanceTraveled;
        distanceToGoalText.text = "Safe Haven: " + Mathf.Max(0, distanceRemaining).ToString("F0") + "m";
        
   
        if (Time.frameCount % 300 == 0) 
        {
            Debug.Log($"Distance Progress - Player Z: {player.position.z:F1}, Start Z: {startZ:F1}, Traveled: {totalDistanceTraveled:F1}, Remaining: {distanceRemaining:F1}");
        }
        

        if (distanceRemaining <= 0 && !isGameOver)
        {
            Debug.Log("Victory condition met! Triggering victory...");
            ShowVictory();
        }

     
        float deltaZ = player.position.z - lastCoinZ;
        Coins += deltaZ * coinMultiplier;
        lastCoinZ = player.position.z; // Update coin tracking position
        coinsText.text = "Coins: " + Mathf.FloorToInt(Coins);

        // Update speed meter
        if (carController != null && speedMeterText != null)
        {
            // Use effective speed (includes lane change penalties) for accurate display
            float currentSpeed = Mathf.Abs(carController.GetEffectiveSpeed());
            float maxSpeed = UpgradeManager.Instance.GetCurrentMaxSpeed();
            
            // Apply speed boost if active
            if (speedBoostActive)
            {
                maxSpeed *= speedBoostMultiplier;
            }
            
            // Convert to a more readable unit (multiply by ~3.6 to simulate km/h)
            float displaySpeed = currentSpeed * 3.6f;
            float displayMaxSpeed = maxSpeed * 3.6f;
            
            speedMeterText.text = $"Speed: {displaySpeed:F0}/{displayMaxSpeed:F0} km/h";
        }

        // Shield timer
        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;

            if (shieldTimerText != null)
            {
                shieldTimerText.gameObject.SetActive(true);
                shieldTimerText.text = "Shield: " + shieldTimer.ToString("F1") + "s";
            }

            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }
        else
        {
            if (shieldTimerText != null)
            {
                shieldTimerText.gameObject.SetActive(false);
            }
        }

        // Speed boost timer
        if (speedBoostActive)
        {
            speedBoostTimer -= Time.deltaTime;

            if (speedBoostTimerText != null)
            {
                speedBoostTimerText.gameObject.SetActive(true);
                speedBoostTimerText.text = "Speed Boost: " + speedBoostTimer.ToString("F1") + "s";
            }

            if (speedBoostTimer <= 0f)
            {
                DeactivateSpeedBoost();
            }
        }
        else
        {
            if (speedBoostTimerText != null)
            {
                speedBoostTimerText.gameObject.SetActive(false);
            }
        }
    }



    public void ShowGameOver()
    {
        isGameOver = true;

        if (crashSource != null && crashClip != null)
        {
            crashSource.PlayOneShot(crashClip);
        }

        if (cameraShake != null)
        {
            cameraShake.TriggerShake();
        }

        FindObjectOfType<CarController>()?.StopMotorSounds();

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        isGameOver = true;
        
        // Calculate and save best completion time
        float completionTime = Time.time - gameStartTime;
        float bestTime = PlayerPrefs.GetFloat("BestCompletionTime", float.MaxValue);
        
        bool isNewBestTime = completionTime < bestTime;
        if (isNewBestTime)
        {
            PlayerPrefs.SetFloat("BestCompletionTime", completionTime);
            Debug.Log($"New best time! {FormatTime(completionTime)} (Previous: {FormatTime(bestTime)})");
        }
        else
        {
            Debug.Log($"Completion time: {FormatTime(completionTime)} (Best: {FormatTime(bestTime)})");
        }
        
        // Display completion time on victory screen
        if (victoryTimeText != null)
        {
            string timeDisplay = $"Completion Time: {FormatTime(completionTime)}";
            if (bestTime != float.MaxValue)
            {
                timeDisplay += $"\nBest Time: {FormatTime(bestTime)}";
                if (isNewBestTime)
                {
                    timeDisplay += "\n🏆 NEW RECORD! 🏆";
                }
            }
            else
            {
                timeDisplay += "\n🏆 FIRST COMPLETION! 🏆";
            }
            victoryTimeText.text = timeDisplay;
        }
        
        Time.timeScale = 0f;
        victoryPanel.SetActive(true);
        FindObjectOfType<CarController>()?.StopMotorSounds();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetProgressAndRestart()
    {
        // Only reset best time if desired - upgrades reset automatically now
        RestartGame();
    }

    // Helper method to format time nicely
    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds == float.MaxValue) return "None";
        
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // Public method to get best completion time
    public float GetBestCompletionTime()
    {
        return PlayerPrefs.GetFloat("BestCompletionTime", float.MaxValue);
    }

    // Public method to get current game time
    public float GetCurrentGameTime()
    {
        return Time.time - gameStartTime;
    }



    public void ActivateShield(float duration)
    {
        // If shield is already active, just extend the timer instead of replacing
        if (shieldActive)
        {
            shieldTimer = Mathf.Max(shieldTimer, duration); // Take the longer duration
            Debug.Log($"Shield extended! New duration: {shieldTimer:F1}s");
            return;
        }

        // Clean up any existing shield visual first
        if (activeShield != null)
        {
            Destroy(activeShield);
            activeShield = null;
        }

        // Create new shield visual
        if (shieldVisualPrefab != null && player != null)
        {
            activeShield = Instantiate(shieldVisualPrefab, player);
            activeShield.transform.localPosition = Vector3.zero;
            activeShield.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("Shield visual prefab or player reference missing!");
        }

        shieldTimer = duration;
        shieldActive = true;
        
        Debug.Log($"Shield activated for {duration:F1} seconds!");
    }

    public void DeactivateShield()
    {
        if (activeShield != null)
        {
            Destroy(activeShield);
            activeShield = null;
        }

        shieldActive = false;
        shieldTimer = 0f;

        if (shieldTimerText != null)
        {
            shieldTimerText.gameObject.SetActive(false);
        }
    }

    public bool IsShieldActive()
    {
        return shieldActive;
    }

    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        // If speed boost is already active, extend duration if the new one is longer
        if (speedBoostActive)
        {
            speedBoostTimer = Mathf.Max(speedBoostTimer, duration);
            speedBoostMultiplier = Mathf.Max(speedBoostMultiplier, multiplier); // Take the higher multiplier
            Debug.Log($"Speed boost extended! Multiplier: {speedBoostMultiplier:F1}x, Duration: {speedBoostTimer:F1}s");
            return;
        }

        speedBoostMultiplier = multiplier;
        speedBoostTimer = duration;
        speedBoostActive = true;
        
        // Notify CarController about the speed boost
        CarController carController = FindObjectOfType<CarController>();
        if (carController != null)
        {
            carController.SetSpeedBoost(speedBoostMultiplier);
        }
        
        Debug.Log($"Speed boost activated! Multiplier: {speedBoostMultiplier:F1}x for {duration:F1} seconds!");
    }

    public void DeactivateSpeedBoost()
    {
        speedBoostActive = false;
        speedBoostTimer = 0f;
        speedBoostMultiplier = 1f;

        if (speedBoostTimerText != null)
        {
            speedBoostTimerText.gameObject.SetActive(false);
        }

        // Notify CarController to reset speed boost
        CarController carController = FindObjectOfType<CarController>();
        if (carController != null)
        {
            carController.SetSpeedBoost(1f); // Reset to normal speed
        }

        Debug.Log("Speed boost deactivated!");
    }

    public bool IsSpeedBoostActive()
    {
        return speedBoostActive;
    }

    public float GetSpeedBoostMultiplier()
    {
        return speedBoostMultiplier;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
    }

    public bool CanAfford(int amount)
    {
        return Coins >= amount;
    }

    public void SpendCoins(int amount)
    {
        if (CanAfford(amount))
        {
            Coins -= amount;
        }
    }
}
