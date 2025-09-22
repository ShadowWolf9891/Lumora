using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[Header("References"),SerializeField]
	Transform groundedCheckObject;

	

	[Header("Movement defaults"),SerializeField]
	float moveSpeed = 6;
	[SerializeField]
	float maxSpeed = 7;
	[SerializeField]
	float stoppingForce = 3;
	[SerializeField]
	float playerHeight = 1.2f;
	[SerializeField]
	float jumpHeight = 5;

	//Private variables
	private InputAction moveAction, attackAction, interactAction, crouchAction, jumpAction;
	private bool shouldFaceMoveDirection = true;
	private LayerMask groundMask;
	private Rigidbody rB;
	private Vector2 moveInput;

	Transform cameraTransform;


	private void Start()
	{
		//TODO: Find correct action using a reference instead of a string
		moveAction = InputSystem.actions.FindAction("Move");
		attackAction = InputSystem.actions.FindAction("West");
		interactAction = InputSystem.actions.FindAction("North");
		crouchAction = InputSystem.actions.FindAction("East");
		jumpAction = InputSystem.actions.FindAction("South");
		
		groundMask = LayerMask.GetMask("Ground");
		rB = GetComponent<Rigidbody>();

		cameraTransform = Camera.main.transform;
	}

	private void Update()
	{
		GetPlayerInputs();
		MovePlayer();
		HandleSpeedControl();
	}

	private void GetPlayerInputs()
	{
		//TODO: Add Game Event raises.
		moveInput = moveAction.ReadValue<Vector2>();

		if (attackAction.WasPressedThisFrame())
		{
			Debug.Log("Attack Action!");
		}
		if (interactAction.WasPressedThisFrame())
		{
			
		}
		if (crouchAction.WasPressedThisFrame())
		{
			Debug.Log("Crouch Action!");
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
	/// <summary>
	/// Checks if the player is on the ground or not.
	/// </summary>
	/// <returns></returns>
	private bool IsGrounded()
	{
		return Physics.Raycast(transform.position, Vector3.down, playerHeight, groundMask);
	}

	private void MovePlayer()
	{
		//Calculates proper move direction
		Vector3 camForward = cameraTransform.forward;
		Vector3 camRight = cameraTransform.right;
		camForward.y = 0f;
		camRight.y = 0f;
		camForward.Normalize();
		camRight.Normalize();
		Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;
		//Movement
		if (moveDirection != Vector3.zero)
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
			//Add drag (the force not the race)
			Vector3 dragForce = new Vector3(-rB.linearVelocity.x * stoppingForce, 0, -rB.linearVelocity.z * stoppingForce);
			rB.AddForce(dragForce, ForceMode.Force);
		}

	}
	/// <summary>
	/// Clamp velocity to the max speed.
	/// </summary>
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
