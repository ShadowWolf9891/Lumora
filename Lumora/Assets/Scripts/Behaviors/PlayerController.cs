using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

	//Private variables
	private InputAction moveAction, sprintAction, interactAction, crouchAction, jumpAction, throwAction, lookAction;
	private Vector2 moveInput;
    private Transform cameraTransform;
	private bool canMove;

    private void Start()
	{
		GameEvents<ChangeGameStateEvent>.Subscribe(FreezePlayer);
		//GameContext.Instance.OnPauseGame += FreezePlayer;
		//GameContext.Instance.OnUnPauseGame += UnFreezePlayer;

		//TODO: Find correct action using a reference instead of a string
		moveAction = InputSystem.actions.FindAction("Move");
		throwAction = InputSystem.actions.FindAction("Right Trigger");
		interactAction = InputSystem.actions.FindAction("North");
		sprintAction = InputSystem.actions.FindAction("West");
		crouchAction = InputSystem.actions.FindAction("East");
		jumpAction = InputSystem.actions.FindAction("South");
		lookAction = InputSystem.actions.FindAction("Look");
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
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("dialogueInteract", PlayerInputActionType.NextDialogue, true));
				//GameContext.Instance.RaiseNextDialogueLine();
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
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("look",PlayerInputActionType.Look));
				//GameContext.Instance.RaiseCameraMove(cameraTransform);
			}
            //if (attackAction.WasPressedThisFrame())
            //{
            //	GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent(PlayerInputActionType.Attack, true));
            //	//GameContext.Instance.RaiseAttack();
            //}
			if (sprintAction.WasPressedThisFrame())
            {
                GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("sprint", PlayerInputActionType.Sprint, true));
            }
            if (interactAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("interact", PlayerInputActionType.Interact, true));
			}
			if (crouchAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("crouch", PlayerInputActionType.Hide, true));
				//GameContext.Instance.RaiseHidePressed();
			}
			if (jumpAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("jump", PlayerInputActionType.Jump, true));
				//GameContext.Instance.RaiseJumpPressed();
			}
			if (throwAction.WasReleasedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("throwRelease", PlayerInputActionType.ThrowRelease, false));
				//GameContext.Instance.RaiseThrowReleased();
			}
			if (throwAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("throw", PlayerInputActionType.Throw, true));
				//GameContext.Instance.RaiseThrowPressed();
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

		GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("move", PlayerInputActionType.Move, default, moveDirection));

	}
	private void FreezePlayer(ChangeGameStateEvent e) 
	{
		if (e.State == GameStates.Running)
		{
			canMove = true;
		}
		else if (e.State == GameStates.Paused || e.State == GameStates.Dialogue) //Not sure what to do with cutscenes yet
		{
			canMove = false;
		}
	}
}
