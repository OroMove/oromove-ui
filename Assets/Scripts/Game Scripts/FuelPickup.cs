using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    public CarController car;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        car.fuelLevel = 1;
        Destroy(gameObject);
    }
}
