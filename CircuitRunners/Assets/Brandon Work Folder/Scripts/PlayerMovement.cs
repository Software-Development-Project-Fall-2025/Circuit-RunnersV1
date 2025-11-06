using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class playerMovement : MonoBehaviour
{
    CharacterController controller;
    [SerializeField] float constSpeed = 5, sprintBuffer = 2.5f, acceleration = 0.015f, jumpHeight = 2.0f;
    float currentSpeed = 0;
    [SerializeField] LayerMask groundLayer;

    // AudioManager audioManager; // Reference to the AudioManager to play sound effects

    //private void Awake()
    // {
    // audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    //}

    Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    bool IsGrounded()
    {
        Vector3 playerFoot = transform.position - new Vector3(0, 1, 0);
        return Physics.CheckSphere(playerFoot, 0.15f, groundLayer);
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (IsGrounded())
            velocity.y = -4f;
        else
            velocity.y += -9.81f * Time.deltaTime;

        // basic sprinting
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (currentSpeed < constSpeed * sprintBuffer)
            {
                currentSpeed += acceleration;
            }
            else
            {
                currentSpeed = constSpeed * sprintBuffer;
            }
        }
        else
        {
            if (currentSpeed > constSpeed)
            {
                currentSpeed -= acceleration;
            }
            else
            {
                currentSpeed = constSpeed;
            }
        }

        // allow for basic strafing
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // apply gravity
        controller.Move(velocity * Time.deltaTime);

        JumpMovement();
    }

    void JumpMovement()
    {
        if (IsGrounded())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * (-9.81f));
                // audioManager.PlaySoundEffects(audioManager.jumpSound);
            }
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
//EndScript