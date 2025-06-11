using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public Transform player;
    public float destroyDistanceBehind = 30f;

    void Update()
    {
        if (player == null) return;

        if (transform.position.z < player.position.z - destroyDistanceBehind)
        {
            Destroy(gameObject);
        }
    }
}