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
    public float fwdSpeed = 200f;  
    // public float[] gearSpeeds = {25, 50, 75, 60, 40, -30};
    // public float maxSpeed = 100f;     
    public float turnSpeed = 200f;

    // Dynamic car variables for easy access
    private float currentSpeed;     // also can be sphereRB.velocity
    private float currentTurnAngle;
    // private float currentAcceleration;
    // private int gearIndex;
    //public float accelerationRate = 5f;



    public Rigidbody sphereRB;
    void Start()
    {
        sphereRB.transform.parent = null;
    }

    void Update()
    {   

        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        // int moveIntensity = 0;
        // if(moveIntensity<10 && moveIntensity>-10)
        //     moveIntensity += (int)moveInput;
        // if(moveInput == 0){
        //     if(moveIntensity>=-10 && moveIntensity<0) moveIntensity+=1;
        //     else if (moveIntensity<=10 && moveIntensity>0) moveIntensity-=1;
        // }
        transform.position = sphereRB.transform.position;

        // // I think this might be more optimal then a switch statement
        // currentSpeed = Vector3.Dot(transform.forward, sphereRB.velocity);
        // //float speedPercent =  Mathf.Clamp01(currentSpeed / maxSpeed); 
        // // currentVelocity = 
        // float speedPercent =  Mathf.Clamp01(sphereRB.velocity.magnitude / maxSpeed); 
        // Debug.Log("Speed percent: " + speedPercent + ", velocity: " + sphereRB.velocity.magnitude + " max speed: " + maxSpeed);
        // gearIndex = Mathf.FloorToInt(speedPercent * (gearSpeeds.Length-1));
        // //Debug.Log("CurrentSpeed: " + currentSpeed + ", Current gear: " + gearIndex);
        // if (gearIndex >= gearSpeeds.Length-1) gearIndex = gearSpeeds.Length - 2;
        // if ((int)moveInput<0) gearIndex = gearSpeeds.Length;         
        // currentAcceleration += gearSpeeds[gearIndex] * moveIntensity;
        // Debug.Log("Gear Index: " + gearIndex);
        
        // if (currentSpeed >= maxSpeed) currentAcceleration = 0;
        moveInput *= fwdSpeed; 
        
        float newRotation = turnInput * turnSpeed * Time.deltaTime * Input.GetAxisRaw("Vertical");
        transform.Rotate(0,newRotation,0 ,Space.World);

    }

    private void FixedUpdate() {

        
        // I should do velocity here not addForce
        sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        // sphereRB.AddForce(transform.forward * currentAcceleration, ForceMode.Accleration);
        // sphereRB.velocity = transform.forward * currentAcceleration;
        //Debug.Log("current acceleration: " + currentAcceleration);

    }

}