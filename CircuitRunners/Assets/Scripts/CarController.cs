using System;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    //private float moveInput;

    //public float fwdSpeed;

    public Rigidbody sphereRB;
    void Start()
    {

    }

    void Update()
    {
        //moveInput = Input.GetAxisRaw("Vertical");
        transform.position = sphereRB.transform.position;
        //moveInput *= fwdSpeed; 
    }

    // private void FixedUpdate() {
    //     sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
    // }

}
