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
	private bool canMove;
	private bool sprinting;
	private Camera mainCam;

    private void Start()
	{
		GameEvents<ChangeGameStateEvent>.Subscribe(OnGameStateChanged);
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
		mainCam = Camera.main;
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
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("look",PlayerInputActionType.Look, true, lookAction.ReadValue<Vector2>()));
			}
			if (sprintAction.WasPressedThisFrame() && !sprinting)
            {
				sprinting = true;
                GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("sprint", PlayerInputActionType.Sprint, sprinting));
            }
			else if(sprintAction.WasReleasedThisFrame() && sprinting)
			{
				sprinting = false;
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("sprint", PlayerInputActionType.Sprint, sprinting));
			}
            if (interactAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("interact", PlayerInputActionType.Interact, true));
			}
			if (crouchAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("crouch", PlayerInputActionType.Hide, true));
			}
			if (jumpAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("jump", PlayerInputActionType.Jump, true));
			}
			if (throwAction.WasReleasedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("throwRelease", PlayerInputActionType.ThrowRelease, false));
			}
			if (throwAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("throw", PlayerInputActionType.Throw, true));
			}
		}
	}
	private void MovePlayer()
	{
		//calculates proper move direction
		Vector3 camForward = mainCam.transform.forward;
		Vector3 camRight = mainCam.transform.right;
		camForward.y = 0f;
		camRight.y = 0f;
		camForward.Normalize();
		camRight.Normalize();
		Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

		GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("move", PlayerInputActionType.Move, default, moveDirection));

	}
	private void OnGameStateChanged(ChangeGameStateEvent e) 
	{
		if (e.State == GameStates.Running)
		{
			canMove = true;
		}
		else if (e.State == GameStates.Paused || e.State == GameStates.Dialogue || e.State == GameStates.Game_Over) //Not sure what to do with cutscenes yet
		{
			canMove = false;
		}
	}
}
