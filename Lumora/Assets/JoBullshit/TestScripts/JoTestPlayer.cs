using System;
using UnityEngine;
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
    public float moveSpeed, jumpHeight;

    private CharacterController characterController;
    private Rigidbody rB;
    private Vector2 moveInput;
    private Transform groundedCheckObject;
    public Transform cameraTransform;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        //
        attackAction = InputSystem.actions.FindAction("West");
        interactAction = InputSystem.actions.FindAction("North");
        crouchAction = InputSystem.actions.FindAction("East");
        jumpAction = InputSystem.actions.FindAction("South");
        groundedCheckObject = transform.Find("GroundedCheckObject").transform;
        //
        characterController = GetComponent<CharacterController>();
        rB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GetPlayerInputs();
        MovePlayer();
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
            Debug.Log("Jump Action!");
            if (Physics.CheckSphere(groundedCheckObject.position,0.1f))
            {
                rB.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
            }
        }
    }

    private void MovePlayer()
    {
        //does all movement to rigidbody attached to CharacterController
        //grab camera properties
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        //move character based on move direction
        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        characterController.Move(new Vector3(0, Physics.gravity.y * Time.deltaTime, 0));
        //rotate character to face move direction, if there is a move input
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion toRotate = Quaternion.LookRotation(moveDirection, Vector3.up);
            //smoothing with Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotate, 100f * Time.deltaTime);
        }
    }
}
