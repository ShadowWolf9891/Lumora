using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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
	[SerializeField] private float snapDistance = 0.6f;
	[SerializeField] private float detectDistance = 1f;
	[SerializeField] private float stealthSpeedModifier = 0.5f;
    [SerializeField] private float sprintNoiseMade = 5f;


    Vector3 startVelocity = Vector3.zero;

	//Private properties
	private HideController hideController;
	bool isHiding;
	public bool isSprinting { get; private set; }
	Rigidbody rb;
	private GameObject coverObject;
	private Vector3 lastWallNormal = Vector3.zero;
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
        GameEvents<EnterStealthEvent>.Subscribe(EnterHide);
        GameEvents<LeaveStealthEvent>.Subscribe(LeaveHide);
		GameEvents<UnlockAbilityEvent>.Subscribe(UnlockAbility);
        //GameContext.Instance.OnMove += Move;
        //GameContext.Instance.OnCameraLook += UpdateThrow;
        //GameContext.Instance.OnAttackPressed += Attack;
        // GameContext.Instance.OnInteractPressed += Interact;
        //GameContext.Instance.OnHidePressed += DoHide;
        //GameContext.Instance.OnJumpPressed += Jump;
        //GameContext.Instance.OnPlayerSpotted += GetSpotted;
        //GameContext.Instance.OnThrowPressed += PrepareThrow;
        //GameContext.Instance.OnThrowReleased += ReleaseThrow;
        //GameContext.Instance.OnEnterHideState += EnterHide;
        //GameContext.Instance.OnLeaveHideState += LeaveHide;

    }

    private void GetComponentReferences()
    {
        rb = GetComponent<Rigidbody>();
        hideController = GetComponent<HideController>();
        if (hideController == null)
        {
            Debug.LogError("HideController not found on player!");
        }
		mainCam = Camera.main;
	}

    private void Update()
    {//i stg if we're trying to remove this specific Update() im gonna crash out -jo
        HandleSpeedControl();
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
			case PlayerInputActionType.Hide:
				DoHide();
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
		if (isHiding)
		{
			CrouchMove(moveDirection);
		}
		else
        {
			if (isSprinting)
			{
				SprintMove(moveDirection);
			}
			else
			{
				DefaultMove(moveDirection);
			}
            FaceMoveDirection(moveDirection);
            //adding drag while grounded
        }

        if (isThrowing) { UpdateThrow(Vector3.zero); }
    }
    private void DoSprint()
    {
		//Called via HandleInput(). Starts player sprinting that continues until player stops moving.
		if (isHiding)
		{
			GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
		}

		if (!isSprinting)
		{
			isSprinting = true;
		}
		else isSprinting = false;
    }
    private void DefaultMove(Vector3 moveDirection)
	{
		rb.AddForce(acceleration * Time.deltaTime * 60 * moveDirection, ForceMode.Acceleration);
	}
    private void SprintMove(Vector3 moveDirection)
    {
		rb.AddForce(acceleration * Time.deltaTime * 60 * moveDirection, ForceMode.Acceleration);
    }
	public void TriggerSprintNoise()
    {
        //GameEvents<SpawnVisibleNoiseEvent>.Raise(new SpawnVisibleNoiseEvent("VisibleNoise", true, transform.position, sprintNoiseMade));
	}
    private void CrouchMove(Vector3 moveDirection)
	{
		Vector3 nextPosition = transform.position + moveDirection;

		if (coverObject == null) return;

		Collider currentCollider = hideController.GetClosestCollider(transform.position)?.GetComponent<Collider>();
		if (currentCollider == null) return;

		// Get wall contact point and normal
		Vector3 wallPoint = currentCollider.ClosestPoint(transform.position);
		Vector3 wallNormal = (transform.position - wallPoint).normalized;
		if (wallNormal.sqrMagnitude > 0.0001f)
			lastWallNormal = wallNormal; // Cache for stability

		Vector3 projectedNextPosition = nextPosition - lastWallNormal * Vector3.Dot(nextPosition - wallPoint, lastWallNormal);

		Vector3 movementAlongPlane = (projectedNextPosition - transform.position);
		rb.AddForce(acceleration * Time.deltaTime * 60 * stealthSpeedModifier * movementAlongPlane, ForceMode.Acceleration);

		float distanceToWall = Vector3.Dot(transform.position - wallPoint, lastWallNormal);
		if (Mathf.Abs(distanceToWall - snapDistance) > 0.01f)
		{
			Vector3 snapTarget = transform.position - lastWallNormal * (distanceToWall - snapDistance);
			rb.MovePosition(Vector3.Lerp(transform.position, snapTarget, 0.5f));
		}

		FaceMoveDirection(moveDirection);
		////Debug for crouch movement, uncomment to re-enable.
		//Debug.DrawLine(transform.position, projectedNextPosition, Color.green);
		//Debug.DrawLine(transform.position, currentCollider.ClosestPoint(transform.position), Color.red);
		//Debug.DrawLine(nextPosition, currentCollider.ClosestPoint(nextPosition), Color.red);
		//Debug.DrawLine(currentCollider.ClosestPoint(transform.position), currentCollider.ClosestPoint(nextPosition), Color.orange);
		//Debug.DrawLine(transform.position,transform.position + wallNormal, Color.blue);
	}

	/// <summary>
	/// Checks if the player is on the ground or not.
	/// </summary>
	/// <returns></returns>
	private bool IsGrounded()
	{
		Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z), UnityEngine.Color.azure);
        //return Physics.Raycast(transform.position, Vector3.down, playerHeight, LayerMask.NameToLayer("Ground"));
        return Physics.Raycast(transform.position, Vector3.down, playerHeight);
    }
	private void FaceMoveDirection(Vector3 moveDirection)
	{
		Quaternion rotateTo = Quaternion.LookRotation(moveDirection, Vector3.up);
		rb.rotation = Quaternion.Slerp(rb.rotation, rotateTo, 10f * Time.deltaTime);
	}

	/// <summary>
	/// Clamp velocity to the max speed.
	/// </summary>
	private void HandleSpeedControl()
	{
		float speedMod = isHiding ? stealthSpeedModifier : 1f;

		Vector3 groundSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
		if (isSprinting && groundSpeed.magnitude > sprintMaxSpeed)
        {
            Vector3 limitedVelocity = groundSpeed.normalized * sprintMaxSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
		else if (!isSprinting && groundSpeed.magnitude > maxSpeed * speedMod)
		{
			Vector3 limitedVelocity = groundSpeed.normalized * maxSpeed * speedMod;
			rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
		}

        if (IsGrounded() && rb.linearVelocity.magnitude > 0.1f)
        {
            rb.AddForce(-stoppingForce * Time.deltaTime * 60 * transform.forward, ForceMode.Acceleration);
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
		//TODO: Raycast to see if the player is interacting with something
	}

    #region Stealth
    /// <summary>
    /// Checks the area for gameobjects with colliders and enters hiding state
    /// </summary>
    private void TryHide()
	{
		Collider closest = hideController.GetClosestCollider(transform.position);
		coverObject = closest ? closest.gameObject : null;

		if (coverObject != null)
        {
			//Toggle hiding
			GameEvents<EnterStealthEvent>.Raise(new EnterStealthEvent("enter_Stealth"));
        }
        else
		{
			GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
		}
	}

	/// <summary>
	/// Triggers on button press. Player tries to hide if not hiding, untoggles hiding state when hiding.
	/// </summary>
    private void DoHide()
    {
		if (!isHiding)
        {
			TryHide();
        }
        else
        {
			GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
			//GameContext.Instance.RaiseLeaveStealth();
        }
    }
	/// <summary>
	/// behavior for when player is spotted. Runs via gamecontext event
	/// </summary>
	private void GetSpotted(PlayerSpottedEvent e)
    {
		GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
        //GameContext.Instance.RaiseLeaveStealth();
    }
    private void LeaveHide(LeaveStealthEvent e)
    {
        isHiding = false;
        // TODO: Add animation
    }

    private void EnterHide(EnterStealthEvent e)
    {
        isSprinting = false;
        isHiding = true;
        // TODO: Add animation
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
