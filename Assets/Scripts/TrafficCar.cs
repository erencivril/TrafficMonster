using UnityEngine;

public class TrafficCar : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float MoveSpeed { get { return moveSpeed; } }
    public float destroyDistanceBehindPlayer = 20f;
    
    private Transform player;
    private int currentLaneIndex = -1;
    private bool isRegistered = false;

    // This method will be called by the spawner to give this car its speed and lane
    public void Initialize(float speed, int laneIndex)
    {
        moveSpeed = speed;
        currentLaneIndex = laneIndex;
        
        // Register with the lane manager
        if (TrafficLaneManager.Instance != null)
        {
            TrafficLaneManager.Instance.RegisterCar(this, currentLaneIndex);
            isRegistered = true;
        }
    }

    private void Start()
    {
        // We still need a reference to the player for the cleanup check
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // If not initialized by spawner, determine lane from position
        if (currentLaneIndex == -1 && TrafficLaneManager.Instance != null)
        {
            currentLaneIndex = TrafficLaneManager.Instance.GetLaneIndex(transform.position.x);
            TrafficLaneManager.Instance.RegisterCar(this, currentLaneIndex);
            isRegistered = true;
        }
    }

    private void Update()
    {
        // Move forward, in the same direction as the player
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Cleanup check: If we are far enough behind the player, destroy this object.
        if (player != null && transform.position.z < player.position.z - destroyDistanceBehindPlayer)
        {
            DestroyTrafficCar();
        }
    }
    
    private void DestroyTrafficCar()
    {
        // Unregister from lane manager before destroying
        if (isRegistered && TrafficLaneManager.Instance != null)
        {
            TrafficLaneManager.Instance.UnregisterCar(this, currentLaneIndex);
            isRegistered = false;
        }
        
        Destroy(gameObject);
    }
    
    private void OnDestroy()
    {
        // Safety cleanup in case DestroyTrafficCar wasn't called
        if (isRegistered && TrafficLaneManager.Instance != null)
        {
            TrafficLaneManager.Instance.UnregisterCar(this, currentLaneIndex);
        }
    }
}