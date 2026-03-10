using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls when the user presses a button, and nothing else.
/// </summary>
public class InputManager : MonoBehaviour
{
	public static InputManager Instance;
	//Private variables
	private InputAction moveAction, sprintAction, interactAction, crouchAction, jumpAction, throwAction, lookAction, restartAction, consoleAction, backAction, pauseAction;
	private Vector2 moveInput;
	private bool sprinting;
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);
	}
	private void Start()
	{
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
		restartAction = InputSystem.actions.FindAction("South");
		consoleAction = InputSystem.actions.FindAction("Console");
		backAction = InputSystem.actions.FindAction("East");
		pauseAction = InputSystem.actions.FindAction("Escape");
	}

	private void Update()
	{
		GetPlayerInputs();
	}

	private void GetPlayerInputs()
	{
		//Always possible actions...
		if (consoleAction.WasPressedThisFrame())
		{
			EventManager.Instance.Raise(new ChangeGameStateEvent("Handle_Console",
				GameManager.Instance.CurrentGameState == GameStates.Console ? GameManager.Instance.PreviousGameState : GameStates.Console));
		}

		if (GameManager.Instance.CurrentGameState == GameStates.Dialogue)
		{
			if (interactAction.WasPressedThisFrame())
			{
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("dialogueInteract", PlayerInputActionType.NextDialogue, true));
				//GameContext.Instance.RaiseNextDialogueLine();
			}
		}
		else if(GameManager.Instance.CurrentGameState == GameStates.Running)
		{
			//Actions that cannot be done while paused...
			if (moveAction.IsInProgress())
			{
				moveInput = moveAction.ReadValue<Vector2>();
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("move", PlayerInputActionType.Move, true, moveInput));
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
				GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("crouch", PlayerInputActionType.Crouch, true));
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
	
}
