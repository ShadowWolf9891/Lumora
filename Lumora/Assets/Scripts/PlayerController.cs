using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

	//Private variables
	private InputAction moveAction, attackAction, interactAction, crouchAction, jumpAction;
	private Vector2 moveInput;
    private Transform cameraTransform;


    private void Start()
	{
		//TODO: Find correct action using a reference instead of a string
		moveAction = InputSystem.actions.FindAction("Move");
		attackAction = InputSystem.actions.FindAction("West");
		interactAction = InputSystem.actions.FindAction("North");
		crouchAction = InputSystem.actions.FindAction("East");
		jumpAction = InputSystem.actions.FindAction("South");
		//
		cameraTransform = GameObject.FindGameObjectWithTag("Camera").transform;
	}

	private void Update()
	{
		GetPlayerInputs();
	}

	private void GetPlayerInputs()
	{
		if (moveAction.IsInProgress())
		{
			moveInput = moveAction.ReadValue<Vector2>();
			MovePlayer();
		}
		if (attackAction.WasPressedThisFrame())
		{
			GameContext.Instance.RaiseAttack();
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
}
