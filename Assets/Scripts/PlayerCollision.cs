using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Shield Feedback")]
    public AudioClip shieldHitSound; // Optional sound when shield absorbs damage
    
    private bool isGameOver = false;
    private AudioSource audioSource;

    private void Start()
    {
        // Get or create audio source for shield hit feedback
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shieldHitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Traffic") && !isGameOver)
        {
            // Check if shield is active
            if (GameManager.Instance.IsShieldActive())
            {
                // Shield absorbs the hit - no damage, no shield removal
                Debug.Log("Shield absorbed traffic collision!");
                
                // Play shield hit sound for feedback
                if (audioSource != null && shieldHitSound != null)
                {
                    audioSource.PlayOneShot(shieldHitSound);
                }
                
                // Add visual feedback (screen shake or flash)
                if (GameManager.Instance.cameraShake != null)
                {
                    GameManager.Instance.cameraShake.TriggerShake(); // Fixed - no parameters needed
                }
                
                // Shield stays active - just return without game over
                return;
            }

            // No shield protection - game over
            isGameOver = true;
            GameManager.Instance.ShowGameOver();
        }
    }
}