using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

	//Private variables
	private InputAction moveAction, attackAction, interactAction, crouchAction, jumpAction, throwAction, lookAction;
	private Vector2 moveInput;
    private Transform cameraTransform;
	private bool canMove;

    private void Start()
	{

		GameContext.Instance.OnPauseGame += FreezePlayer;
		GameContext.Instance.OnUnPauseGame += UnFreezePlayer;

		//TODO: Find correct action using a reference instead of a string
		moveAction = InputSystem.actions.FindAction("Move");
		throwAction = InputSystem.actions.FindAction("Right Trigger");
		interactAction = InputSystem.actions.FindAction("North");
		crouchAction = InputSystem.actions.FindAction("East");
		jumpAction = InputSystem.actions.FindAction("South");
		lookAction = InputSystem.actions.FindAction("Look");
		//Add attack back if needed, function is commented out to account for throw mechanic
		//attackAction = InputSystem.actions.FindAction("");
		cameraTransform = GameObject.FindGameObjectWithTag("Camera").transform;
	}

	private void Update()
	{
		GetPlayerInputs();
	}

	private void GetPlayerInputs()
	{
		//Always possible actions...

		if (!canMove)
		{
			if (interactAction.WasPressedThisFrame())
			{
				GameContext.Instance.RaiseNextDialogueLine();
			}
		}
		else
		{
			//Actions that cannot be done while paused...
			if (moveAction.IsInProgress())
			{
				moveInput = moveAction.ReadValue<Vector2>();
				MovePlayer();
			}
			if (lookAction.IsInProgress())
			{
				GameContext.Instance.RaiseCameraMove(cameraTransform);
			}
			//if (attackAction.WasPressedThisFrame())
			{
				//GameContext.Instance.RaiseAttack();
			}
			if (interactAction.WasPressedThisFrame())
			{
				GameContext.Instance.RaiseInteract();
			}
			if (crouchAction.WasPressedThisFrame())
			{
				GameContext.Instance.RaiseHidePressed();
			}
			if (jumpAction.WasPressedThisFrame())
			{
				GameContext.Instance.RaiseJumpPressed();
			}
			if (throwAction.WasReleasedThisFrame())
			{
				GameContext.Instance.RaiseThrowReleased();
			}
			if (throwAction.WasPressedThisFrame())
			{
				GameContext.Instance.RaiseThrowPressed();
			}
		}
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

		GameContext.Instance.RaiseMove(moveDirection);

	}
	private void FreezePlayer() { canMove = false; }
	private void UnFreezePlayer() { canMove = true; }
}
