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
    public float fwdSpeed = 200;  
    //public float[] gearSpeeds = {-50, 25, 50, 75, 60, 40};
    //public float maxSpeed = 200;
    public float turnSpeed = 200;

    // Dynamic car variables for easy access
    private float currentSpeed;
    private float currentTurnAngle;


    public Rigidbody sphereRB;
    void Start()
    {
        sphereRB.transform.parent = null;
    }

    void Update()
    {   
        // need to put code for adjustable speed in here

        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");


        transform.position = sphereRB.transform.position;
        // Meldin gonna kill me for this >_> (ill optimize later)
        // if()
        moveInput *= fwdSpeed; 
        
        float newRotation = turnInput * turnSpeed * Time.deltaTime * Input.GetAxisRaw("Vertical");
        transform.Rotate(0,newRotation,0 ,Space.World);

    }

    private void FixedUpdate() {
        sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
    }

}
