using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject currentInteractable;
    private InputAction moveAction, attackAction, interactAction, crouchAction, jumpAction;
    public float playerHeight, moveSpeed, maxSpeed, stoppingForce, jumpHeight;
    private bool shouldFaceMoveDirection = true, canInteract = false;

    private LayerMask groundMask;
    private Vector3 verticalVelocity;


    private Rigidbody rB;
    private Vector2 moveInput;

    public Transform groundedCheckObject;
    public Transform cameraTransform;
    
    List<Collider> coverInRange = new List<Collider>();
    [SerializeField]
    bool behindCoverMovement;
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
            RunInteractionEvent();
        }
        if (crouchAction.WasPressedThisFrame() && coverInRange.Count >= 1)
        {
            if (!behindCoverMovement && coverInRange.Count >= 0)
            {
                //find nearest collider 
                Collider nearestCover = coverInRange.First();
                foreach (Collider c in coverInRange)
                {
                    float distanceToC = Vector3.Distance(c.ClosestPointOnBounds(transform.position), transform.position);
                    if (distanceToC < Vector3.Distance(nearestCover.ClosestPointOnBounds(transform.position), transform.position))
                    {
                        nearestCover = c;
                    }

                }
                Debug.Log($"Finding Nearest cover, {nearestCover.gameObject.name} is nearest");
                TakeCover(nearestCover);
            }
            else
            {
                LeaveCover();
            }

        }
        if (jumpAction.WasPressedThisFrame())
        {
            if (IsGrounded())
            {
                Debug.Log("Jump Action!");
                rB.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
            }
        }
    }

    //we can run events here!
    private void TakeCover(Collider nearestCover)
    {
        behindCoverMovement = true;
        Vector3 directionToCover = -(transform.position - nearestCover.ClosestPointOnBounds(transform.position));
        rB.AddForce(directionToCover, ForceMode.Impulse);
    }
    private void LeaveCover()
    {
        behindCoverMovement = false;
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
        if (moveDirection!= Vector3.zero && !behindCoverMovement)
        {
            rB.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }
        //Movement behind cover
        else if (moveDirection != Vector3.zero && behindCoverMovement)
        {
            moveDirection.Normalize();

            //finding valid movement directions
            Vector3 closestValidMoveDirection = new Vector3(0, 0, 0);
            foreach (Collider c in coverInRange)
            {
                //raycasts to colliders near player
                RaycastHit ray;
                if (Physics.Raycast(transform.position, -(transform.position - c.ClosestPointOnBounds(transform.position)), out ray))
                {
                    //find 2 directions paralell to normal hit
                    Vector3 hitNormal = ray.normal;
                    Debug.DrawLine(ray.point, ray.point + hitNormal, Color.darkOrange);
                    Vector3 direction1 = Quaternion.Euler(0, 90, 0) * hitNormal;
                    Vector3 direction2 = Quaternion.Euler(0, -90, 0) * hitNormal;
                    direction1.Normalize();
                    direction2.Normalize();

                    //finds valid direction closest to moveDirection vector
                    if (Vector3.Distance(direction1, moveDirection) < Vector3.Distance(closestValidMoveDirection, moveDirection))
                    {
                        closestValidMoveDirection = direction1;
                    }
                    if (Vector3.Distance(direction2, moveDirection) < Vector3.Distance(closestValidMoveDirection, moveDirection))
                    {
                        closestValidMoveDirection = direction2;
                    }
                    Debug.DrawLine(transform.position, transform.position + direction1, Color.blue);
                    Debug.DrawLine(transform.position, transform.position + direction2, Color.blue);
                }
            }

            Debug.DrawLine(transform.position, transform.position + closestValidMoveDirection, Color.green);
            //after finding best direction, moves player at half speed (note 5f multiplier instead of 10).
            rB.AddForce(closestValidMoveDirection * moveSpeed * 5f, ForceMode.Force);
        }

        //facing character to movement
        if (shouldFaceMoveDirection)
        {
            FaceMoveDirection(moveDirection);
        }

        //adding drag while grounded
        if (IsGrounded() && rB.linearVelocity.magnitude > 0.1f)
        {
            Vector3 dragForce = new Vector3(-rB.linearVelocity.x * stoppingForce, 0, -rB.linearVelocity.z * stoppingForce);
            rB.AddForce(dragForce, ForceMode.Force);
            //Debug.Log($"Running Stopping force, dragForce = {dragForce.x}, {dragForce.z}");
        }

    }

    private void FaceMoveDirection(Vector3 moveDirection)
    {
        Quaternion rotateTo = Quaternion.LookRotation(moveDirection, Vector3.up);
        rB.rotation = Quaternion.Slerp(rB.rotation, rotateTo, 10f * Time.deltaTime);
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
    

    //All interaction stuff is designed around the prototype!!! we need to redo this!!!!!
    private void RunInteractionEvent()
    {
        if (currentInteractable != null)
        {
            Debug.Log($"Interacted with {currentInteractable.name}");
            Destroy(currentInteractable);
        }
        else
        {
            Debug.Log("Nothing to interact with!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // works on default layer
        if (other.gameObject.layer == 0)
        {
            Debug.Log("added to cover in range");
            coverInRange.Add(other);
        }
        if (other.gameObject.CompareTag("Collectable"))
        {
            canInteract = true;
            currentInteractable = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // works on default layer
        if (other.gameObject.layer == 0 && coverInRange.Contains(other))
        {
            Debug.Log("removed cover in range");
            coverInRange.Remove(other);
            if (coverInRange.Count == 0)
            {
                LeaveCover();
            }
        }
        // if theres nothing in radius and last thing leaves
        if (other.gameObject.CompareTag("Collectable"))
        {
            canInteract = false;
            currentInteractable = null;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (behindCoverMovement)
        {
            Vector3 moveLinePoint = transform.position;
            moveLinePoint.x += moveInput.x;
            moveLinePoint.z += moveInput.y;
            Gizmos.DrawLine(transform.position, moveLinePoint);
        }
    }
}
