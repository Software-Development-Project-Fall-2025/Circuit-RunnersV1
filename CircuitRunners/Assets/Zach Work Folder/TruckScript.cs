// Zachary Schupbach
// Fall 25
// Credit for base mechanics 
// https://www.youtube.com/watch?v=TBIYSksI10k

using System;
using UnityEngine;
// 
/*
Mechanics I want
Can turn really well at lower speeds, higher speeds oversteer
When you turn too much at higher speed (after oversteering/drifting a bunch) you spin out
Gradual Acceleration
Variables to easily control everything
*/

/*
 This code is currently 35mph ish through turns, you can get up to high 30s
 before oversteer starts taking over, (might be to much)
 max speed of 42-44 still doesnt spin you out
*/
public class TruckScript : MonoBehaviour {
    private float moveInput;
    private float turnInput;


    public float health = 100f;
    public float maxSpeed = 33f;
    public float turnSpeed = 160f;
    public float baseAcceleration = 100f;

    public float lateralForce = 40f;
    public float driftFactor = 0.8f;
    public float driftStartAngle = 25f;
    public float spinoutAngle = 65f;
    public float spinoutTorque = 20f;
    public float grip = 60f;

    public float riseResponse = 3f;
    public float fallResponse = 2f;

    private float moveIntensity;
    private bool isDrifting = false;
    private bool isSpinningOut = false;
    private bool isOnRoad = true;  // Track surface type

    public float raycastDistance = 1f; 
    public string roadTag = "Road";         
    public string grassTag = "Grass";       
    public float roadGripMultiplier = 1f;
    public float grassGripMultiplier = .4f;
    public float grassIceSlip = 0.7f; // 0 = no slip, 1 = very slippery

    public Rigidbody sphereRB;

    void Start() {
        sphereRB.constraints = RigidbodyConstraints.FreezePositionY;
        //sphereRB.constraints = RigidbodyConstraints.FreezeRotationX;
        //sphereRB.constraints = RigidbodyConstraints.FreezeRotationZ;
        //sphereRB.constraints = RigidbodyConstraints.FreezeRotationY;
        sphereRB.transform.parent = null;

    }

    void FixedUpdate() {

        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");
        transform.position = sphereRB.transform.position;

        // I swear this makes it drive better although it does a stupid lil nose dip
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(sphereRB.velocity.normalized, Vector3.up), Time.fixedDeltaTime * 2f);


        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        float forwardSpeed = Vector3.Dot(sphereRB.velocity, forward);
        float speedPercent = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);

        // Smooths input
        float tau = moveInput != 0f ? riseResponse : fallResponse;
        float alpha = 1f - Mathf.Exp(-Time.fixedDeltaTime / tau);
        moveIntensity += (moveInput - moveIntensity) * alpha;
        // Debug.Log("Move Intensity: " + moveIntensity);

        float accelMultiplier = magicTruckTranny(speedPercent);
        float rawForce = moveIntensity * baseAcceleration * accelMultiplier;
        float speedLimitFactor = 1f - Mathf.Pow(speedPercent, 4);
        float targetForce = rawForce * speedLimitFactor;


        if (!(forwardSpeed > maxSpeed && targetForce > 0) &&
            !(forwardSpeed < -maxSpeed * 0.5f && targetForce < 0))
        {
            sphereRB.AddForce(forward * targetForce, ForceMode.Acceleration);
        }

        // Gets surface type with raycast
        RaycastHit hit;
        isOnRoad = false;  
        
        LayerMask groundMask = ~LayerMask.GetMask("PlayerTruck");
        
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, raycastDistance, groundMask))
        {
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            string tagName = hit.collider.gameObject.tag;
            //Debug.Log("Layer: " + layerName + " | Tag: " + tagName);
            
            if (tagName == roadTag)
            {
                isOnRoad = true;
            }
            else
            {
                // You on the grass lmao
            }
        }


        float currentGripMultiplier = isOnRoad ? roadGripMultiplier : grassGripMultiplier;
        
        float turnMultiplier = Mathf.Lerp(1f, 0.4f, speedPercent) * currentGripMultiplier;
        float newRotation = turnInput * turnSpeed * turnMultiplier * Time.fixedDeltaTime;
        transform.Rotate(0, newRotation, 0, Space.World);

        Vector3 vel = sphereRB.velocity;
        if (vel.sqrMagnitude > 0.1f)
        {
            float slipAngle = Vector3.SignedAngle(forward, vel.normalized, Vector3.up);
            float absSlip = Mathf.Abs(slipAngle);

            isDrifting = absSlip > driftStartAngle && absSlip < spinoutAngle;
            isSpinningOut = absSlip >= spinoutAngle;

            if (isDrifting)
            {
                Debug.Log("Drifting");
                vel = Vector3.Lerp(vel, forward * vel.magnitude, driftFactor * Time.fixedDeltaTime);
                sphereRB.velocity = vel;
                sphereRB.AddForce(right * turnInput * lateralForce * speedPercent, ForceMode.Acceleration);
            }
            else if (isSpinningOut)
            {
                Debug.Log("spinning out");
                float spinDir = Mathf.Sign(turnInput != 0 ? turnInput : slipAngle);
                sphereRB.AddTorque(Vector3.up * spinDir * spinoutTorque, ForceMode.Acceleration);
            }
            else
            {
                float lateralSpeed = Vector3.Dot(sphereRB.velocity, right);
                float currentGrip = grip * (isOnRoad ? roadGripMultiplier : grassGripMultiplier);
                Vector3 gripForce = -right * lateralSpeed * currentGrip;
                sphereRB.AddForce(gripForce, ForceMode.Acceleration);

                if (!isOnRoad)
                {
                    Debug.Log("slipping");
                    //sphereRB.AddForce(vel.normalized * grassIceSlip, ForceMode.Acceleration);
                    float spinDir = Mathf.Sign(turnInput != 0 ? turnInput : slipAngle);
                    sphereRB.AddTorque(Vector3.up * spinDir * spinoutTorque, ForceMode.Acceleration);
                }
            }
        }

        // Adjust drag based on surface and speed
        float baseDrag = isOnRoad ? 0.2f : 0.4f;  // More drag on grass
        float maxDrag = isOnRoad ? 2f : 3f;       // More max drag on grass
        sphereRB.drag = Mathf.Lerp(baseDrag, maxDrag, speedPercent); // Adds all the oversteer 
    }

    private float magicTruckTranny(float speedPercent){
        // Definitley needs another gear but its fucky
        if (speedPercent < 0.4f)
            return Mathf.Lerp(0.3f, 0.7f, speedPercent / 0.4f);
        else if (speedPercent < 0.7f)
            return Mathf.Lerp(0.7f, 0.9f, (speedPercent - 0.3f) / 0.3f);
        else
            return 1.0f;
    }
}
