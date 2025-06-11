using UnityEngine;

public class ShieldPowerUp : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float collectionRadius = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 50f; // Rotation for visual appeal
    

    
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
        // Rotate for visual appeal
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
            CollectShield();
        }
    }

    private void CollectShield()
    {
        // Mark as collected immediately to prevent double collection
        hasBeenCollected = true;
        
        // Disable collider to prevent further triggers
        if (powerUpCollider != null)
        {
            powerUpCollider.enabled = false;
        }

        // Activate shield in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActivateShield(duration);
        }
        else
        {
            Debug.LogError("GameManager.Instance is null! Cannot activate shield.");
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, collectionRadius);
    }

    // Public method to set duration (useful for different shield power-up variants)
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    // Public method to check if this power-up has been collected
    public bool IsCollected()
    {
        return hasBeenCollected;
    }
}