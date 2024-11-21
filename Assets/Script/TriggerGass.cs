using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGass : MonoBehaviour
{
    public DriverCar car;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        car.Refuel(20);// Adds 1 unit of fuel; adjust this value as needed
        Destroy(gameObject);
    }
}
