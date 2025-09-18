using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class JoTestPlayer : MonoBehaviour
{
    /*
     * Hey! Jo making a test script for the player here.
     * I'm planning on doing a 3rd person camera that rotates around the player,
     * player movement, player basic interactions, 
     * and defining a zone around the player that finds the nearest wall when prompted.
     */
    private InputAction moveAction, attackAction, interactAction, crouchAction, jumpAction;
    public float playerHeight, moveSpeed, maxSpeed, stoppingForce, jumpHeight;
    private bool shouldFaceMoveDirection = true;

    private LayerMask groundMask;
    private Vector3 verticalVelocity;


    private CharacterController characterController;
    private Rigidbody rB;
    private Vector2 moveInput;
    public Transform groundedCheckObject;
    public Transform cameraTransform;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        //
        attackAction = InputSystem.actions.FindAction("West");
        interactAction = InputSystem.actions.FindAction("North");
        crouchAction = InputSystem.actions.FindAction("East");
        jumpAction = InputSystem.actions.FindAction("South");
        //
        groundMask = LayerMask.GetMask("Ground");
        characterController = GetComponent<CharacterController>();
        rB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GetPlayerInputs();
        MovePlayer();
        HandleSpeedControl();
    }

    private void GetPlayerInputs()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (attackAction.WasPressedThisFrame())
        {
            Debug.Log("Attack Action!");
        }
        if (interactAction.WasPressedThisFrame())
        {
            Debug.Log("Interact Action!");
        }
        if (crouchAction.WasPressedThisFrame())
        {
            Debug.Log("Crouch Action!");
        }
        if (jumpAction.WasPressedThisFrame())
        {
            if (IsGrounded())
            {
                rB.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, playerHeight, groundMask);
    }

    private void MovePlayer()
    {
        //calculates proper move direction
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;
        //movement
        if (moveDirection!= Vector3.zero)
        {
            rB.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
            
            if (shouldFaceMoveDirection)
            {
                Quaternion rotateTo = Quaternion.LookRotation(moveDirection, Vector3.up);
                rB.rotation = Quaternion.Slerp(rB.rotation, rotateTo, 10f * Time.deltaTime);
            }
        }
        else if (IsGrounded() && rB.linearVelocity.magnitude > 0.1f)
        {
            //add drag (the force not the race)
            Vector3 dragForce = new Vector3(-rB.linearVelocity.x * stoppingForce, 0, -rB.linearVelocity.z * stoppingForce);
            rB.AddForce(dragForce, ForceMode.Force);
            Debug.Log($"Running Stopping force, dragForce = {dragForce.x}, {dragForce.z}");
        }

    }

    private void HandleSpeedControl()
    {
        Vector3 groundSpeed = new Vector3(rB.linearVelocity.x, 0, rB.linearVelocity.z);
        if (groundSpeed.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = groundSpeed.normalized * maxSpeed;
            rB.linearVelocity = new Vector3(limitedVelocity.x, rB.linearVelocity.y, limitedVelocity.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z));
    }
}
