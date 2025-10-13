using System;
using System.Collections.Generic;
using UnityEngine;

// Credit for base mechanics 
// https://www.youtube.com/watch?v=TBIYSksI10k

/*
Mechanics I want
Can turn really well at lower speeds, higher speeds oversteer
When you turn too much at higher speed (after oversteering/drifting a bunch) you spin out
Gradual Acceleration
Variables to easily control everything
*/
public class CarController : MonoBehaviour
{
    // Inputs from user
    private float moveInput;
    private float turnInput;

    // Static car Variables (Stats)
    public float maxSpeed = 50f;     
    public float turnSpeed = 175f;
    public float baseAcceleration = 250f;   // This also controls max speed

    public float lateralForce = 40f;      // sideways force for drifting
    public float driftFactor = 0.8f;
    private bool isDrifting = false;
    public float riseResponse = 4f;    
    public float fallResponse = 4f;

    // Dynamic Car variables
    private float moveIntensity;


    public Rigidbody sphereRB;
    void Start()
    {
        sphereRB.transform.parent = null;
    }

    void Update()
    {   

    }

    void FixedUpdate()
    {
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        transform.position = sphereRB.transform.position;

        // Get our directional vectors (car according to plane) and speeds
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        float forwardSpeed = Vector3.Dot(sphereRB.velocity, forward);
        float speedPercent = Mathf.Abs(forwardSpeed) / maxSpeed;


        //input and speed smoothening
        float tau = moveInput != 0f ? riseResponse : fallResponse;
        float alpha = 1f - Mathf.Exp(-Time.fixedDeltaTime / tau);
        moveIntensity += (moveInput - moveIntensity) * alpha;

        float accelMultiplier = magicTranny(speedPercent);
        float rawForce = moveIntensity * baseAcceleration * accelMultiplier;

        float speedLimitFactor = 1f - Mathf.Pow(speedPercent, 4); 
        float targetForce = rawForce * speedLimitFactor;

        // Only apply force if we're not exceeding speed limits
        if (!(forwardSpeed > maxSpeed && targetForce > 0) && 
            !(forwardSpeed < -maxSpeed * 0.5f && targetForce < 0))
        {
            sphereRB.AddForce(transform.forward * targetForce, ForceMode.Acceleration);
        }


        // Reduces turn speed at high speeds
        float turnMultiplier = Mathf.Lerp(1f, 0.5f, speedPercent);
        float newRotation = turnInput * turnSpeed * turnMultiplier * Time.fixedDeltaTime;
        transform.Rotate(0, newRotation, 0, Space.World);

        // Need to make a real traction element
        isDrifting = Mathf.Abs(turnInput) > 0.5f && speedPercent > driftFactor;
        Debug.Log("Drifting: " + isDrifting);
        if (isDrifting){    
            sphereRB.AddForce(right * turnInput * lateralForce * speedPercent, ForceMode.Acceleration);
        }
        else{
            float lateralSpeed = Vector3.Dot(sphereRB.velocity, right);
            Vector3 gripForce = -right * lateralSpeed * 80f; // higher = more grip
            sphereRB.AddForce(gripForce, ForceMode.Acceleration);
        }

        Debug.Log("Current Speed: " + forwardSpeed);
    }


    private float magicTranny(float speedPercent){
        if (speedPercent < 0.3f)
            return Mathf.Lerp(0.3f, 0.7f, speedPercent / 0.3f);  
        else if (speedPercent < 0.6f)
            return Mathf.Lerp(0.7f, 1.0f, (speedPercent - 0.3f) / 0.3f);  
        else
            return 1.0f;  



        // if (speedPercent < 0.15f)
        //     return Mathf.Lerp(0.01f, 0.2f, speedPercent / 0.15f);
        // else if (speedPercent < 0.4)
        //     return Mathf.Lerp(0.1f, 0.8f, (speedPercent-0.15f) / 0.25f);
        // else if (speedPercent < 0.65f)
        //     return Mathf.Lerp(0.7f, 1.0f, (speedPercent - 0.4f) / 0.25f);
        // else if (speedPercent < 0.85f)
        //     return Mathf.Lerp(0.9f, 0.4f, (speedPercent - 0.8f) / 0.2f);
        // else
        //     return Mathf.Lerp(0.3f, 0.1f, (speedPercent - 0.85f) / 0.15f);
    }
}