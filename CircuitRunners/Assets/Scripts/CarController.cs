using System;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Vector3 input;
    void GatherInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        input = new Vector3(steerInput, 0, moveInput);
    }
    
    public enum Axel
    {
        Front,
        Rear
    }

    [System.Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public float maxAcceleration = 30f;
    public float brakeAcceleration = 50f;
    public float turnSensitivity = 1f;
    public float maxSteeringAngle = 30f;

    public Vector3 centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = centerOfMass;
    }

    void Update()
    {
        GatherInputs();
    }

    void LateUpdate()
    {
        Move();
        Steer();
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                float steerAngle = steerInput * maxSteeringAngle * turnSensitivity;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, 0.6f);
            }
        }
    }
}
