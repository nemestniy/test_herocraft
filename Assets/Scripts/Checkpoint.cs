using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Car car;
    private void Awake()
    {
        car = GameManager.Instance.Car;
    }

    private void Update()
    {
        var distance = (car.transform.position - transform.position).magnitude;
        if (distance < transform.localScale.magnitude)
        {
            car.IncreaseScore();
            Destroy(gameObject);
        }
    }
}
