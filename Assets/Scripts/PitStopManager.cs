using UnityEngine;
using TMPro;

public class PitStopManager : MonoBehaviour
{
    public static PitStopManager Instance;

    [Header("Pit Stop Settings")]
    public GameObject pitStopPrefab;
    public float pitStopDistance = 1000f; // The Z-distance between pit stops
    public float[] laneXPositions = { -3.3f, 0f, 3.3f }; // The X positions for the three lanes
    [Tooltip("How far past the pit stop the player must go to trigger a 'skip'.")]
    public float skipThreshold = 20f;

    [Header("UI")]
    public TextMeshProUGUI distanceText;

    private GameObject currentPitStop;
    private int currentPitStopLane; // 0=left, 1=center, 2=right
    private bool pitStopActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        SpawnNextPitStop();
    }

    void Update()
    {
        if (pitStopActive && currentPitStop != null)
        {
            float distanceRemaining = currentPitStop.transform.position.z - GameManager.Instance.player.transform.position.z;
            distanceText.text = "Pit Stop: " + Mathf.Max(0, distanceRemaining).ToString("F0") + "m";

            // --- Check if the player has SKIPPED the pit stop ---
            if (distanceRemaining < -skipThreshold)
            {
                Debug.Log("Player skipped the pit stop. Spawning next one.");
                SpawnNextPitStop(); // Respawn a new pit stop further ahead
            }
        }
    }

    public void SpawnNextPitStop()
    {
        // Clean up old pit stop if it exists
        if (currentPitStop != null)
        {
            Destroy(currentPitStop);
        }

        // --- Pick a random lane ---
        currentPitStopLane = Random.Range(0, laneXPositions.Length);
        float spawnX = laneXPositions[currentPitStopLane];
        
        Vector3 spawnPosition = new Vector3(spawnX, 0, GameManager.Instance.player.transform.position.z + pitStopDistance);
        currentPitStop = Instantiate(pitStopPrefab, spawnPosition, Quaternion.identity);
        
        pitStopActive = true;
        distanceText.gameObject.SetActive(true);
    }

    public void PlayerReachedPitStop()
    {
        if (!pitStopActive) return;

        pitStopActive = false;
        distanceText.gameObject.SetActive(false);

        // Pause the game and open shop for upgrades and police heat reset
        Time.timeScale = 0f;
        if (GameManager.Instance.shopUI != null)
        {
            GameManager.Instance.shopUI.OpenShop();
        }
    }

    public void PitStopEscape()
    {
        // Called when player continues from pit stop
        SpawnNextPitStop();
    }

    // --- Public Getters for Traffic Spawner ---
    public bool IsPitStopActive()
    {
        return pitStopActive;
    }

    public Vector3 GetCurrentPitStopPosition()
    {
        if (currentPitStop != null)
        {
            return currentPitStop.transform.position;
        }
        return Vector3.zero; // Return a zero vector if no pit stop exists
    }

    public int GetCurrentPitStopLaneIndex()
    {
        return currentPitStopLane;
    }
} 