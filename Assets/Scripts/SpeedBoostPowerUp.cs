using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float duration = 8f; // How long the speed boost lasts
    [SerializeField] private float speedMultiplier = 1.5f; // 50% speed increase
    [SerializeField] private float collectionRadius = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 100f; // Faster rotation for speed theme
    
    
    private bool hasBeenCollected = false;
    private Collider powerUpCollider;
    private AudioSource audioSource;

    private void Start()
    {
        // Get components
        powerUpCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        
        // Ensure we have a trigger collider
        if (powerUpCollider != null)
        {
            powerUpCollider.isTrigger = true;
        }
        
    }

    private void Update()
    {
        // Rotate for visual appeal (faster than shield for speed theme)
        if (!hasBeenCollected)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent multiple collections
        if (hasBeenCollected)
        {
            return;
        }

        // Check if it's the player
        if (other.gameObject.CompareTag("Player"))
        {
            CollectSpeedBoost();
        }
    }

    private void CollectSpeedBoost()
    {
        // Mark as collected immediately to prevent double collection
        hasBeenCollected = true;
        
        // Disable collider to prevent further triggers
        if (powerUpCollider != null)
        {
            powerUpCollider.enabled = false;
        }

        // Activate speed boost in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActivateSpeedBoost(speedMultiplier, duration);
        }
        else
        {
            Debug.LogError("GameManager.Instance is null! Cannot activate speed boost.");
        }

 


        // Hide visual
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Destroy the power-up after a short delay 
        Destroy(gameObject, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // Yellow for speed
        Gizmos.DrawWireSphere(transform.position, collectionRadius);
    }

    // Public method to set duration (useful for different speed boost variants)
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    // Public method to set speed multiplier
    public void SetSpeedMultiplier(float newMultiplier)
    {
        speedMultiplier = newMultiplier;
    }

    // Public method to check if this power-up has been collected
    public bool IsCollected()
    {
        return hasBeenCollected;
    }
} 