using UnityEngine;
using TMPro;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 20f; // Speed gained per second
    public float deceleration = 30f; // Speed lost per second when braking/reversing
    public float maxSpeed = 30f;
    public float maxReverseSpeed = 10f; // The maximum speed when reversing
    public float laneChangeSpeed = 15f;
    public float laneOffset = 3.3f;
    [Tooltip("How fast the car loses speed when no keys are pressed.")]
    public float friction = 2f; 

    [Header("Lane Settings")]
    public float[] laneXPositions = { -3.3f, 0f, 3.3f }; // Left, Center, Right lane positions

    [Header("UI")]
    public TextMeshProUGUI reverseIndicator;

    [Header("Audio")]
    public AudioSource motorLow;
    public AudioSource motorMid;
    public AudioSource motorHigh;
    public float basePitch = 1f;
    public float pitchRange = 0.5f;
    public float maxPitchSpeed = 30f;
    public float baseVolume = 0.3f;
    public float lowThreshold = 12f;
    public float highThreshold = 20f;

    // Private state variables
    private float moveSpeed;
    public float MoveSpeed { get { return moveSpeed; } } // Public getter for other scripts
    private int currentLane = 0; // -1 for left, 0 for center, 1 for right
    private float handlingPenalty = 1.0f; // The multiplier for speed loss on lane change
    private bool isChangingLanes = false;
    private float laneChangeSpeedMultiplier = 1f;

    // Speed boost variables
    private float speedBoostMultiplier = 1f;

    void Start()
    {
        // --- Apply Upgrades from UpgradeManager ---
        if (UpgradeManager.Instance != null)
        {
            maxSpeed = UpgradeManager.Instance.GetCurrentMaxSpeed();
            laneChangeSpeed = UpgradeManager.Instance.GetCurrentHandling();
            handlingPenalty = UpgradeManager.Instance.GetCurrentHandlingPenalty();
        }

        // Hide the reverse indicator at the start
        if (reverseIndicator != null)
            reverseIndicator.gameObject.SetActive(false);

        if (motorLow) motorLow.Play();
        if (motorMid) motorMid.Play();
        if (motorHigh) motorHigh.Play();
    }

    void Update()
    {
        HandleInput();
        MoveForward();
        UpdateMotorSounds();
    }

    void HandleInput()
    {
        bool wasChangingLanes = isChangingLanes;

        if (Input.GetKeyDown(KeyCode.A) && !isChangingLanes)
        {
            currentLane = Mathf.Max(0, currentLane - 1);
            isChangingLanes = true;
        }
        else if (Input.GetKeyDown(KeyCode.D) && !isChangingLanes)
        {
            currentLane = Mathf.Min(2, currentLane + 1);
            isChangingLanes = true;
        }

        float targetX = laneXPositions[currentLane];
        if (isChangingLanes)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(targetX, transform.position.y, transform.position.z),
                laneChangeSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
            {
                isChangingLanes = false;
                transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
            }
        }

        // Apply speed penalty while changing lanes - reduced by handling upgrades
        if (isChangingLanes)
        {
            laneChangeSpeedMultiplier = UpgradeManager.Instance.GetCurrentHandlingPenalty();
            
            if (handlingPenalty != laneChangeSpeedMultiplier)
            {
                Debug.Log($"Lane change penalty: {(1f - laneChangeSpeedMultiplier) * 100f:F0}% speed loss (Handling Level {UpgradeManager.Instance.HandlingLevel})");
                handlingPenalty = laneChangeSpeedMultiplier; // Cache for next frame comparison
            }
        }
        else
        {
            laneChangeSpeedMultiplier = 1f; // Normal speed when not changing lanes
        }
    }

    private void MoveForward()
    {
        if (Input.GetKey(KeyCode.W))
        {
            moveSpeed += acceleration * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveSpeed -= acceleration * Time.deltaTime;
        }
        else
        {
            // No input, gradually decrease speed 
            if (moveSpeed > 0)
            {
                moveSpeed -= deceleration * Time.deltaTime;
                moveSpeed = Mathf.Max(moveSpeed, 0); // Don't go below 0
            }
            else if (moveSpeed < 0)
            {
                moveSpeed += deceleration * Time.deltaTime;
                moveSpeed = Mathf.Min(moveSpeed, 0); // Don't go above 0
            }
        }

        float boostedMaxSpeed = maxSpeed * speedBoostMultiplier;
        
        moveSpeed = Mathf.Clamp(moveSpeed, -maxReverseSpeed, boostedMaxSpeed);

        float effectiveSpeed = moveSpeed * laneChangeSpeedMultiplier;

        Vector3 newPosition = transform.position;
        
        newPosition.z += effectiveSpeed * Time.deltaTime;


        float rearBoundaryZ = RoadSpawner.Instance.GetRearmostZPosition();
   
        newPosition.z = Mathf.Max(newPosition.z, rearBoundaryZ);

        transform.position = newPosition;
        
        // --- Update Reverse Indicator UI ---
        if (reverseIndicator != null)
        {
            reverseIndicator.gameObject.SetActive(moveSpeed < -0.1f);
        }
    }

    private void UpdateMotorSounds()
    {
        if (motorLow == null) return; 

        float absSpeed = Mathf.Abs(moveSpeed);
        float normalizedSpeed = Mathf.Clamp01(absSpeed / maxPitchSpeed);
        float currentPitch = basePitch + normalizedSpeed * pitchRange;

        motorLow.pitch = currentPitch;
        motorMid.pitch = currentPitch;
        motorHigh.pitch = currentPitch;

        float targetLow = 0f, targetMid = 0f, targetHigh = 0f;

        if (absSpeed < lowThreshold)
        {
            targetLow = baseVolume;
        }
        else if (absSpeed < highThreshold)
        {
            float t = Mathf.InverseLerp(lowThreshold, highThreshold, absSpeed);
            targetLow = Mathf.Lerp(baseVolume, 0f, t);
            targetMid = Mathf.Lerp(0f, baseVolume, t);
        }
        else
        {
            float t = Mathf.InverseLerp(highThreshold, maxSpeed, absSpeed);
            targetMid = Mathf.Lerp(baseVolume, 0f, t);
            targetHigh = Mathf.Lerp(0f, baseVolume, t);
        }

        motorLow.volume = Mathf.Lerp(motorLow.volume, targetLow, Time.deltaTime * 5f);
        motorMid.volume = Mathf.Lerp(motorMid.volume, targetMid, Time.deltaTime * 5f);
        motorHigh.volume = Mathf.Lerp(motorHigh.volume, targetHigh, Time.deltaTime * 5f);
    }

    public void StopMotorSounds()
    {
        if (motorLow) motorLow.Stop();
        if (motorMid) motorMid.Stop();
        if (motorHigh) motorHigh.Stop();
    }

    public void SetSpeedBoost(float multiplier)
    {
        speedBoostMultiplier = multiplier;
        Debug.Log($"CarController: Speed boost set to {multiplier:F1}x");
    }

    public float GetSpeedBoostMultiplier()
    {
        return speedBoostMultiplier;
    }
    
    public float GetEffectiveSpeed()
    {
        return moveSpeed * laneChangeSpeedMultiplier;
    }

    public void RefreshUpgradeValues()
    {
        if (UpgradeManager.Instance != null)
        {
            maxSpeed = UpgradeManager.Instance.GetCurrentMaxSpeed();
            laneChangeSpeed = UpgradeManager.Instance.GetCurrentHandling();
            handlingPenalty = UpgradeManager.Instance.GetCurrentHandlingPenalty();
            
            Debug.Log($"CarController: Refreshed upgrade values - Max Speed: {maxSpeed}, Lane Change Speed: {laneChangeSpeed}, Handling Penalty: {handlingPenalty}");
        }
    }
}

