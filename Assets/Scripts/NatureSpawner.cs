using UnityEngine;

public class NatureSpawner : MonoBehaviour
{
    public GameObject[] naturePrefabs;  
    public Transform player;

    public float spawnInterval = 20f;  
    public float spawnDistanceAhead = 60f;
    public float sideOffset = 10f;  

    private float lastSpawnZ;

    private void Start()
    {
        lastSpawnZ = player.position.z;
    }

    private void Update()
    {
        if (player.position.z + spawnDistanceAhead > lastSpawnZ + spawnInterval)
        {
            SpawnDecorations();
            lastSpawnZ += spawnInterval;
        }
    }

    private void SpawnDecorations()
    {
        foreach (GameObject prefab in naturePrefabs)
        {
            for (int i = 0; i < 2; i++)
            {
                float xOffset = Random.Range(12f, 24f) * (Random.value > 0.5f ? 1 : -1);
                float zOffset = Random.Range(-10f, 10f); 
                float spawnZ = player.position.z + spawnDistanceAhead + zOffset;

                Vector3 spawnPos = new Vector3(xOffset, 0f, spawnZ);
                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

                AutoDestroy auto = obj.AddComponent<AutoDestroy>();
                auto.player = player;
            }
        }
    }


}