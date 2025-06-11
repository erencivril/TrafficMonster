using UnityEngine;

public class PitStop : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Find the PitStopManager in the scene and notify it
            PitStopManager manager = FindObjectOfType<PitStopManager>();
            if (manager != null)
            {
                manager.PlayerReachedPitStop();
            }
            
            // Deactivate the pitstop to prevent multiple triggers
            gameObject.SetActive(false);
        }
    }
} 