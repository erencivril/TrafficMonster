using System.Collections.Generic;
using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    public static RoadSpawner Instance;

    public GameObject[] roadPrefabs;
    public int numberOfSegments = 6;
    public float segmentLength = 30f;
    public Transform player;

    private List<GameObject> activeSegments = new List<GameObject>();
    private float spawnZ;
    private int nextPrefabIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        spawnZ = player.position.z - segmentLength;
    }

    void Start()
    {
        for (int i = 0; i < numberOfSegments; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
        
        if (player.position.z > spawnZ - (numberOfSegments - 2) * segmentLength)
        {
            MoveSegmentForward();
        }
    }


    void SpawnSegment()
    {
        GameObject prefabToSpawn = roadPrefabs[nextPrefabIndex];
        Vector3 spawnPosition = new Vector3(0, 0, spawnZ);
        GameObject segment = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        activeSegments.Add(segment);

        spawnZ += segmentLength;
        nextPrefabIndex = (nextPrefabIndex + 1) % roadPrefabs.Length;
    }

    void MoveSegmentForward()
    {
        GameObject firstSegment = activeSegments[0];
        activeSegments.RemoveAt(0);

        Vector3 newPosition = new Vector3(0, 0, spawnZ);
        firstSegment.transform.position = newPosition;
        activeSegments.Add(firstSegment);

        spawnZ += segmentLength;
        nextPrefabIndex = (nextPrefabIndex + 1) % roadPrefabs.Length;
    }

    public float GetRearmostZPosition()
    {
        // The rearmost segment is the first one in our list.
        // Its position is the center, so we subtract half the length to get the back edge.
        if (activeSegments.Count > 0)
        {
            return activeSegments[0].transform.position.z - (segmentLength / 2f);
        }

        // Fallback if no segments are active for some reason
        return player.position.z - 10f;
    }
}