using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerBehavior : MonoBehaviour
{
    #region Properties
    [Header("Player Settings")]
	[SerializeField, Tooltip("How fast the player accelerates to max speed in m/s^2")]
	private float acceleration = 10;
	[SerializeField, Tooltip("The maximum speed of the player in m/s")]
	private float maxSpeed = 4;
    [SerializeField, Tooltip("Acceleration multiplier for sprinting, applies directly to acceleration")]
    private float sprintMaxSpeed = 6;
    [SerializeField, Tooltip("How quickly the player stops moving in m/s")]
	float stoppingForce = 3;
	[SerializeField, Tooltip("Height of the player for jumping in m")]
	float playerHeight = 1.2f;
	[SerializeField, Tooltip("How high the player can jump in m")]
	float jumpHeight = 5;
	[SerializeField, Tooltip("LayerMask for IsGrounded")]
	LayerMask groundedLayers;

    //throw mechanic
    [Header("Throw Settings")]
	[SerializeField] GameObject thrownObjPrefab;
	[SerializeField] Transform throwLocation;
	[SerializeField] float throwForce = 10;
	[SerializeField] float throwSensitivity = 1f;
	//line renderer 
	[SerializeField] LineRenderer lineRenderer;
	[SerializeField] GameObject hitSpherePrefab;
	private GameObject activeHitSphere;
	private int linePoints = 16;
	private float timeBetweenPoints = 0.15f;
	private bool isThrowing;
	private bool canThrow;

	[Header("Stealth Settings")]
    [SerializeField] private float sprintNoiseMade = 5f;
    [SerializeField] private float standingHeight = 1f;
    [SerializeField] private float crouchedHeight = 0.5f;
	[SerializeField] private float stealthSpeedModifier = 0.5f;
	[SerializeField] private float stealthSnapDistance = 0.4f;
	private Collider closestWall = null;

	public bool IsCrouching { get; private set; }
	public bool IsSprinting { get; private set; }

	//Private properties
	Vector3 startVelocity = Vector3.zero;
	//private HideController hideController;
	Rigidbody rb;
	private Camera mainCam;
	private Vector3 curThrowDirection;
	private float throwYaw;
	private float throwPitch;
	#endregion

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Awake()
    {
        SubscribeToEvents();
        GetComponentReferences();
    }
	private void Start()
	{
		CameraManager.SetCurrentCamera("3rd Person Camera");
	}

	private void SubscribeToEvents()
    {
        GameEvents<PlayerInputEvent>.Subscribe(HandleInput);
        GameEvents<PlayerSpottedEvent>.Subscribe(GetSpotted);
		GameEvents<UnlockAbilityEvent>.Subscribe(UnlockAbility);
    }
    private void GetComponentReferences()
    {
        rb = GetComponent<Rigidbody>();
		mainCam = Camera.main;
	}
    private void HandleInput(PlayerInputEvent e)
	{
		switch (e.ActionType)
		{
			case PlayerInputActionType.Move:
				Move(e.MoveDirection);
				break;
			case PlayerInputActionType.Look:
				UpdateThrow(e.MoveDirection);
				break;
			case PlayerInputActionType.Interact:
				Interact();
				break;
			case PlayerInputActionType.Sprint:
				DoSprint();
				break;
			case PlayerInputActionType.Jump:
				Jump();
				break;
			case PlayerInputActionType.Crouch:
				Crouch();
				break;
			case PlayerInputActionType.Throw:
				if (canThrow)
				{
					PrepareThrow();
				}
				break;
			case PlayerInputActionType.ThrowRelease:
				if (canThrow)
				{
					ReleaseThrow();
				}
				break;

		}
	}

    //Contains basic movement, crouched movement, jumping, and all helpers associated
    #region Movement
    private void Move(Vector3 moveDirection)
	{
		Collider newClosest = HideController.GetClosestWall(transform.position);
		if (closestWall != null && closestWall != newClosest)
		{
			closestWall = newClosest;
			rb.MovePosition(HideController.SnapToWall(transform.position, closestWall, stealthSnapDistance));
		}

		Vector3 newDirection = closestWall != null ? HideController.GetHideMovement(transform.position, moveDirection, closestWall) : moveDirection;
		
		//Calculate how fast to move based on state
		float speedScale = 1f;

		if (IsCrouching || closestWall != null) speedScale = stealthSpeedModifier;
		else if (IsSprinting) speedScale = 1.5f;

		rb.AddForce(60 * acceleration * speedScale * Time.fixedDeltaTime * newDirection, ForceMode.Acceleration);
		FaceMoveDirection(newDirection);

		//Check if throwing
		if (isThrowing) { UpdateThrow(Vector3.zero); }

		//Limit speed
		HandleSpeedControl();
	}

    private void DoSprint()
    {
		//Called via HandleInput(). Starts player sprinting that continues until player stops moving.
		if (IsCrouching)
		{
			GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("crouch", PlayerInputActionType.Crouch, true));
		}
		IsSprinting = !IsSprinting;
    }
	public void TriggerSprintNoise()
	{
		GameEvents<SpawnVisibleNoiseEvent>.Raise(new SpawnVisibleNoiseEvent("VisibleNoise", true, transform.position, sprintNoiseMade));
	}
	private void Crouch()
	{
		IsCrouching = !IsCrouching;
	}
	
	/// <summary>
	/// Checks if the player is on the ground or not.
	/// </summary>
	/// <returns></returns>
	private bool IsGrounded()
	{
		Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z), UnityEngine.Color.darkRed);
        return Physics.Raycast(transform.position, Vector3.down, playerHeight, groundedLayers);
    }
	private void FaceMoveDirection(Vector3 moveDirection)
	{
		if (moveDirection.sqrMagnitude < 0.001f) return; //Return since 0 would give error
		Quaternion rotateTo = Quaternion.LookRotation(moveDirection, Vector3.up);
		rb.rotation = Quaternion.Slerp(rb.rotation, rotateTo, 10f * Time.fixedDeltaTime);
	}

	/// <summary>
	/// Clamp velocity to the max speed.
	/// </summary>
	private void HandleSpeedControl()
	{
		float speedMod = IsCrouching ? stealthSpeedModifier : 1f;

		Vector3 groundSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
		if (IsSprinting && groundSpeed.magnitude > sprintMaxSpeed)
        {
            Vector3 limitedVelocity = groundSpeed.normalized * sprintMaxSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
		else if (!IsSprinting && groundSpeed.magnitude > maxSpeed * speedMod)
		{
			Vector3 limitedVelocity = groundSpeed.normalized * maxSpeed * speedMod;
			rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
		}

        if (IsGrounded() && rb.linearVelocity.magnitude > 0.1f)
        {
			Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
			rb.AddForce(-horizontalVel * stoppingForce, ForceMode.Acceleration);
            //Debug.Log($"Running Stopping force, dragForce = {dragForce.x}, {dragForce.z}");
        }
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
        }
    }

    #endregion

    #region Throwing
    private void PrepareThrow()
	{
		//when pressing throw key, creates a line render to show expected trajectory for projectile
		//Debug.Log("Preparing throw.");
		isThrowing = true;
		
		CameraManager.SetCurrentCamera("ThrowCamera", 0.2f);
		throwYaw = mainCam.transform.forward.x;
		throwPitch = -10f; // slight upward bias

	}
	private void UpdateThrow(Vector2 lookInput)
	{
		if (!isThrowing) return;

		Vector3 startPos = throwLocation.position;

		// Update aiming angles
		throwYaw += lookInput.x * throwSensitivity;
		throwPitch -= lookInput.y * throwSensitivity;

		// Clamp vertical aim to avoid flipping
		throwPitch = Mathf.Clamp(throwPitch, -60f, 60f);

		// Convert angles to direction
		Quaternion rotation =
		Quaternion.AngleAxis(throwYaw, Vector3.up) *
		Quaternion.AngleAxis(throwPitch, Camera.main.transform.right);

		curThrowDirection = rotation * transform.forward;
		curThrowDirection.Normalize();

		startVelocity = curThrowDirection.normalized * throwForce;

		Vector3[] points = new Vector3[linePoints];
		lineRenderer.positionCount = linePoints;
		for (int i = 0; i < linePoints; i++)
		{
			float time = i * timeBetweenPoints;

			Vector3 position = startPos
						 + startVelocity * time
						 + 0.5f * time * time * Physics.gravity;
			points[i] = position;
			if (i > 0)
			{
				Vector3 prevPoint = points[i - 1];
				Vector3 dir = position - prevPoint;
				float dist = dir.magnitude;

				if (Physics.Raycast(prevPoint, dir.normalized, out RaycastHit hit, dist))
				{
					if (activeHitSphere == null)
						activeHitSphere = Instantiate(hitSpherePrefab);

					activeHitSphere.transform.position = hit.point;

					// Stop the line at the hit point
					points[i] = hit.point;
					lineRenderer.positionCount = i + 1;
					
					break;
				}
			}

		}
		lineRenderer.SetPositions(points);
		lineRenderer.enabled = true;

	}
	private void ReleaseThrow()
	{
		//releasing the throw key will remove the line render and throw the projectile based on player location (cube attached to player atm)
		//throw direction is based on camera position (forward)
		//Debug.Log("Release Throw");
		isThrowing = false;
		lineRenderer.enabled = false;
		Destroy(activeHitSphere);
		if (!CameraManager.IsBlending())
		{
			GameObject projectile = Instantiate(thrownObjPrefab, throwLocation.position, Quaternion.identity);
			Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
			projectileRb.AddForce(startVelocity, ForceMode.Impulse);
		}
		CameraManager.ReturnToPreviousCamera(0.5f);
	}
	#endregion
	private void Interact()
	{
		//If something to interact with

		//else
		//Set closest wall to null if it have a value, and a value if it was null.
		closestWall = closestWall == null ? HideController.GetClosestWall(transform.position) : null;
		rb.MovePosition(HideController.SnapToWall(transform.position, closestWall, stealthSnapDistance));
	}

    #region Stealth
   
	/// <summary>
	/// behavior for when player is spotted. Runs via gamecontext event
	/// </summary>
	private void GetSpotted(PlayerSpottedEvent e)
    {
		GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
    }
    
	#endregion

	#region EventStuff
	/// <summary>
	/// Event handler for unlocking a specific player ability.
	/// </summary>
	/// <param name="e">The Unlock Ability event defined in a json file.</param>
	private void UnlockAbility(UnlockAbilityEvent e)
	{
		switch (e.AbilityName)
		{
			case "throw":
				canThrow = true;
				break;
			//Add other abilities here

			default:
				Debug.LogError($"Invalid ability name to unlock {e.AbilityName}");
				break;
		}
	}


	#endregion
}
