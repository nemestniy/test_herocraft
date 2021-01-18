using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public event Action Finished;
    public int Score { get; private set; }

    public CarData Data;

    [Header("Settings")]
    public float MaxDistanceFromTrajectory = 30.0f;

    private Vector3[] path;
    private int pointNumber;
    private float currentSpeed;
    private float midSpeed;

    private float acceleration = 0;
    private float braking = 0;
    private bool acceleratingAllowed = true;
    private bool finished = false;

    public void Inititalize(Vector3[] path)
    {
        this.path = path;
        pointNumber = 1;
        midSpeed = Data.MaxSpeed * 0.5f;
        currentSpeed = midSpeed;

        var direction = path[pointNumber+1] - transform.position;
        transform.rotation = Quaternion.LookRotation(direction);

        Score = 0;
    }

    public void Accelerate()
    {
        if (!acceleratingAllowed)
        {
            return;
        }

        if(braking > 0)
            braking = 0;

        acceleration = Mathf.Lerp(acceleration, Data.Acceleration, Time.deltaTime);
        currentSpeed += acceleration;
    }

    public void SetMiddleSpeed()
    {
        braking = 0;
        acceleration = 0;

        currentSpeed = Mathf.Lerp(currentSpeed, midSpeed, Time.deltaTime);
    }

    public void Brake()
    {
        if (acceleration > 0)
            acceleration = 0;

        braking = Mathf.Lerp(braking, Data.Braking, Time.deltaTime);
        currentSpeed -= braking;
    }

    private void Update()
    {
        if (path != null && path.Length > 0 && pointNumber < path.Length - 1)
        {
            Move();

            var distance = Mathf.Abs((path[pointNumber] - transform.position).magnitude);

            if (distance > MaxDistanceFromTrajectory)
            {
                acceleratingAllowed = false;
                Brake();
            }

            if (distance < MaxDistanceFromTrajectory * 0.5f && !acceleratingAllowed)
                acceleratingAllowed = true;

            if (distance < 1.0f || (transform.InverseTransformPoint(path[pointNumber]).z < 0 && distance < MaxDistanceFromTrajectory))
            {
                pointNumber++;
                acceleratingAllowed = true;
            }   
        }

        else if(pointNumber == path.Length - 1 && !finished)
        {
            currentSpeed = 0;
            acceleratingAllowed = false;
            Finished();
            finished = true;
        }

    }

    private void Move()
    {
        var direction = (path[pointNumber] - transform.position).normalized;

        var newRotation = Quaternion.LookRotation(direction);
        var angle = Quaternion.Angle(transform.rotation, newRotation);
        var previousDirection = transform.forward;

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 1 / (currentSpeed * Mathf.Sqrt(Data.Mass)));

        currentSpeed = Mathf.Clamp(currentSpeed, 0, Data.MaxSpeed);
        var resultVector = Vector3.forward;

        transform.Translate(resultVector.normalized * currentSpeed * Time.deltaTime);
    }
    
    public void IncreaseScore()
    {
        Score++;
    }
}

[Serializable]
public struct CarData
{
    public float MaxSpeed;
    public float Mass;
    public float Acceleration;
    public float Braking;
}
