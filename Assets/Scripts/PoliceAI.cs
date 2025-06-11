using UnityEngine;

public class PoliceAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public Transform player;
    [SerializeField] private float baseLaneChangeSpeed = 4f; // Base lane change speed (slower start)
    [SerializeField] private float maxLaneChangeSpeed = 10f; // Maximum lane change speed at high difficulty
    [SerializeField] private float laneChangeChance = 0.015f; // Slightly reduced chance to change lanes
    public float[] laneXPositions = { -3.3f, 0f, 3.3f };
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyScaleRate = 0.5f; // How fast lane changing gets faster per upgrade/minute

    [Header("Position Constraints")]
    [SerializeField] private float minDistanceBehindPlayer = 2f; // Minimum distance to stay behind player - closer chase
    [SerializeField] private float maxDistanceBehindPlayer = 15f; // Maximum distance before catching up faster - tighter range
    
    [Header("Busting Settings")]
    public float bustDistance = 3f; // How close the police car needs to be to bust - closer busting
    public float bustTimerDuration = 2f; // Faster busting for closer chases
    public float sameLaneThreshold = 1.5f; // How close to same X position to be considered "same lane"

    public float BustProgress { get; private set; } // Other scripts can read this to update UI

    private float currentSpeed;
    private float maxSpeed; // Set by PoliceManager
    private int currentLaneIndex;
    private float bustTimer = 0f;
    private CarController playerController;
    private float gameStartTime; // Track when police chase started

    public void Initialize(Transform playerTransform, float policeMaxSpeed)
    {
        player = playerTransform;
        maxSpeed = policeMaxSpeed;
        playerController = player.GetComponent<CarController>();
        gameStartTime = Time.time; // Record when this chase started
        
        // Start in the center lane
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        currentLaneIndex = 1; // Center lane
        
        Debug.Log($"Police initialized with max speed: {maxSpeed}");
    }

    void Update()
    {
        if (player == null || playerController == null || GameManager.Instance.IsGameOver())
        {
            return;
        }

        HandleMovement();
        HandleBusting();
    }

    private void HandleMovement()
    {
        //  Ensure police stays behind player 
        float distanceToPlayer = player.position.z - transform.position.z;
        
        // Calculate target speed based on distance to player
        if (distanceToPlayer < minDistanceBehindPlayer)
        {
            // Too close - slow down but stay aggressive
            currentSpeed = Mathf.Max(playerController.MoveSpeed * 0.7f, 2f);
        }
        else if (distanceToPlayer > maxDistanceBehindPlayer)
        {
            // Too far - speed up aggressively to catch up
            currentSpeed = maxSpeed;
        }
        else
        {
            // Normal chase speed - keep pressure on player
            currentSpeed = Mathf.Min(playerController.MoveSpeed + 2f, maxSpeed);
        }
        
        // Lane following - make it configurable and slower
        int playerLaneIndex = GetLaneIndex(player.position.x);
        
        // Use configurable lane change chance
        if (Random.value < laneChangeChance)
        {
            if (playerLaneIndex != currentLaneIndex)
            {
                // Move towards player's lane
                if (playerLaneIndex > currentLaneIndex)
                {
                    currentLaneIndex = Mathf.Min(currentLaneIndex + 1, 2);
                }
                else if (playerLaneIndex < currentLaneIndex)
                {
                    currentLaneIndex = Mathf.Max(currentLaneIndex - 1, 0);
                }
            }
        }
        
        // Move towards target lane position (using difficulty-scaled speed)
        float currentLaneChangeSpeed = GetCurrentLaneChangeSpeed();
        float targetX = laneXPositions[currentLaneIndex];
        Vector3 position = transform.position;
        position.x = Mathf.MoveTowards(position.x, targetX, currentLaneChangeSpeed * Time.deltaTime);
        
        //  Move forward (ensuring we don't overtake player)
        float newZ = position.z + currentSpeed * Time.deltaTime;
        
        // Never allow police to get in front of player
        float maxAllowedZ = player.position.z - minDistanceBehindPlayer;
        newZ = Mathf.Min(newZ, maxAllowedZ);
        
        position.z = newZ;
        transform.position = position;
    }

    private void HandleBusting()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool inSameLane = Mathf.Abs(transform.position.x - player.position.x) < sameLaneThreshold;
        bool policeBehindPlayer = transform.position.z < player.position.z; // Police should be behind

        // Only bust if close AND in the same lane AND police is behind player
        if (distanceToPlayer <= bustDistance && inSameLane && policeBehindPlayer)
        {
            // Player is in range and same lane, increase the bust timer
            bustTimer += Time.deltaTime;
        }
        else
        {
            // Player is out of range, different lane, or police is somehow in front - decrease the bust timer
            bustTimer -= Time.deltaTime * 0.5f; // Slower decrease makes it more forgiving
        }

        // Clamp the timer between 0 and the max duration
        bustTimer = Mathf.Clamp(bustTimer, 0, bustTimerDuration);

        // Calculate the progress as a 0-1 value
        BustProgress = bustTimer / bustTimerDuration;

        // If the bust timer is full, game over
        if (bustTimer >= bustTimerDuration)
        {
            GameManager.Instance.ShowGameOver();
        }
    }

    private float GetCurrentLaneChangeSpeed()
    {
        // Calculate difficulty-based lane change speed
        float difficultyFactor = 0f;
        
        // Add time-based scaling (gets faster over time in chase)
        float chaseTimeMinutes = (Time.time - gameStartTime) / 60f;
        difficultyFactor += chaseTimeMinutes * 0.3f;
        
        // Add upgrade-based scaling (gets faster as player upgrades)
        if (UpgradeManager.Instance != null)
        {
            int totalUpgrades = UpgradeManager.Instance.GetTotalUpgradeLevel();
            difficultyFactor += totalUpgrades * 0.2f;
        }
        
        // Calculate current speed with scaling
        float scaledSpeed = baseLaneChangeSpeed + (difficultyFactor * difficultyScaleRate);
        
        // Clamp to max speed
        return Mathf.Clamp(scaledSpeed, baseLaneChangeSpeed, maxLaneChangeSpeed);
    }

    private int GetLaneIndex(float xPos)
    {
        // Determine which lane an x-position corresponds to
        if (xPos < -1.65f) return 0; // Left lane
        if (xPos > 1.65f) return 2;  // Right lane
        return 1; // Center lane
    }
    
    // Inspector method to adjust base lane change speed at runtime
    public void SetBaseLaneChangeSpeed(float newSpeed)
    {
        baseLaneChangeSpeed = newSpeed;
    }
    
    // Inspector method to adjust max lane change speed at runtime
    public void SetMaxLaneChangeSpeed(float newSpeed)
    {
        maxLaneChangeSpeed = newSpeed;
    }
    
    // Inspector method to adjust lane change frequency
    public void SetLaneChangeChance(float newChance)
    {
        laneChangeChance = Mathf.Clamp01(newChance);
    }
    
    // Get current lane change speed for debugging
    public float GetCurrentLaneChangeSpeedDebug()
    {
        return GetCurrentLaneChangeSpeed();
    }
} 