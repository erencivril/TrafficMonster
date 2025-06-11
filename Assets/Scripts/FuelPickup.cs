using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    public float fuelAmount = 25f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FuelSystem fuelSystem = other.GetComponent<FuelSystem>();
            if (fuelSystem != null)
            {
                fuelSystem.AddFuel(fuelAmount);


                Destroy(gameObject);
            }
        }
    }
} 