using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
	[Header("Player Settings")]
	[SerializeField, Tooltip("How fast the player accelerates to max speed in m/s^2")]
	private float acceleration = 10;
	[SerializeField, Tooltip("The maximum speed of the player in m/s")]
	private float maxSpeed = 10;
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
	//line renderer 
	[SerializeField] LineRenderer lineRenderer;
	private int linePoints = 8;
	private float timeBetweenPoints = 0.15f;
	private bool isThrowing;

	[Header("Stealth Settings")]
	[SerializeField] private float snapDistance = 0.6f;
	[SerializeField] private float detectDistance = 1f;
	[SerializeField] private float stealthSpeedModifier = 0.5f;


	Vector3 startVelocity = Vector3.zero;

	//Private properties
	private HideController hideController;
	bool isHiding;
	Rigidbody rb;
	private GameObject coverObject;
	private Vector3 lastWallNormal = Vector3.zero;
	private Transform cameraTransform;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		GameEvents<PlayerInputEvent>.Subscribe(HandleInput);
		GameEvents<PlayerSpottedEvent>.Subscribe(GetSpotted);
		GameEvents<EnterStealthEvent>.Subscribe(EnterHide);
		GameEvents<LeaveStealthEvent>.Subscribe(LeaveHide);
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

		rb = GetComponent<Rigidbody>();
		hideController = GetComponent<HideController>();
		cameraTransform = GameObject.FindGameObjectWithTag("Camera").transform;
		if (hideController == null)
		{
			Debug.LogError("HideController not found on player!");
		}
	}

	private void HandleInput(PlayerInputEvent e)
	{
		switch (e.ActionType)
		{
			case PlayerInputActionType.Move:
				Move(e.MoveDirection);
				break;
			case PlayerInputActionType.Look:
				UpdateThrow(cameraTransform);
				break;
			case PlayerInputActionType.Attack:
				Attack();
				break;
			case PlayerInputActionType.Interact:
				Interact();
				break;
			case PlayerInputActionType.Jump:
				Jump();
				break;
			case PlayerInputActionType.Hide:
				DoHide();
				break;
			case PlayerInputActionType.Throw:
				PrepareThrow();
				break;
			case PlayerInputActionType.ThrowRelease:
				ReleaseThrow();
				break;

		}
	}

	private void Move(Vector3 moveDirection)
	{
		if (isHiding)
		{
			CrouchMove(moveDirection);
		}
		else
		{
			DefaultMove(moveDirection);

			if (IsGrounded() && rb.linearVelocity.magnitude > 0.1f)
			{
				rb.AddForce(-stoppingForce * Time.deltaTime * 60 * transform.forward, ForceMode.Acceleration);
				//Debug.Log($"Running Stopping force, dragForce = {dragForce.x}, {dragForce.z}");
			}
		}

		FaceMoveDirection(moveDirection);
		//adding drag while grounded

		HandleSpeedControl();
		if (isThrowing) { UpdateThrow(CameraManager.CurrentCamera.transform); }

	}

	private void DefaultMove(Vector3 moveDirection)
	{
		rb.AddForce(acceleration * Time.deltaTime * 60 * moveDirection, ForceMode.Acceleration);
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

		Debug.DrawLine(transform.position, projectedNextPosition, Color.green);
		Debug.DrawLine(transform.position, currentCollider.ClosestPoint(transform.position), Color.red);
		Debug.DrawLine(nextPosition, currentCollider.ClosestPoint(nextPosition), Color.red);
		Debug.DrawLine(currentCollider.ClosestPoint(transform.position), currentCollider.ClosestPoint(nextPosition), Color.orange);
		Debug.DrawLine(transform.position,transform.position + wallNormal, Color.blue);
	}

	/// <summary>
	/// Checks if the player is on the ground or not.
	/// </summary>
	/// <returns></returns>
	private bool IsGrounded()
	{
		Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - playerHeight, transform.position.z), Color.azure);
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
		if (groundSpeed.magnitude > maxSpeed * speedMod)
		{
			Vector3 limitedVelocity = groundSpeed.normalized * maxSpeed *speedMod;
			rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
		}
	}
	private void Attack()
	{
		Debug.Log("Attack Pressed.");
	}
	
	private void PrepareThrow()
	{
		//when pressing throw key, creates a line render to show expected trajectory for projectile
		//Debug.Log("Preparing throw.");
		isThrowing = true;
		CameraManager.SetCurrentCamera("ThrowCamera", 0.2f);
		UpdateThrow(CameraManager.CurrentCamera.transform);

	}
	private void UpdateThrow(Transform cameraTransform)
	{
		if (isThrowing)
		{
			Vector3 startPos = throwLocation.position;
			startVelocity = (cameraTransform.forward + (cameraTransform.up /2)) * throwForce;
			Vector3[] points = new Vector3[linePoints];
			for (int i = 0; i < linePoints; i++)
			{
				float time = i * timeBetweenPoints;

				Vector3 curVelocity = startVelocity * time;
				Vector3 curAcceleration = 0.5f * Mathf.Pow(time,2f) * Physics.gravity;
				Vector3 position = startPos + curVelocity + curAcceleration;

				points[i] = position;
			}
			lineRenderer.positionCount = linePoints;
			lineRenderer.SetPositions(points);
			lineRenderer.enabled = true;

			Vector3 moveDir = cameraTransform.forward;
			moveDir.y = 0f;

			FaceMoveDirection(moveDir);
		}
	}
	private void ReleaseThrow()
	{
		//releasing the throw key will remove the line render and throw the projectile based on player location (cube attached to player atm)
		//throw direction is based on camera position (forward)
		//Debug.Log("Release Throw");
		isThrowing = false;
		lineRenderer.enabled = false;
		if (!CameraManager.IsBlending())
		{
			GameObject projectile = Instantiate(thrownObjPrefab, throwLocation.position, Quaternion.identity);
			Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
			projectileRb.AddForce(startVelocity, ForceMode.Impulse);
		}
		CameraManager.ReturnToPreviousCamera(0.5f);
	}
	private void Interact()
	{
		//TODO: Raycast to see if the player is interacting with something
	}

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
            //GameContext.Instance.RaiseEnterStealth();
        }
        else
		{
			GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
			//GameContext.Instance.RaiseLeaveStealth();
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
		GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent(e.Id)); //Inproper use of id, fix later

        //GameContext.Instance.RaiseLeaveStealth();
		//give player temporary movespeed buff? players should run away here, right?
    }

	private void Jump()
	{
		if (IsGrounded())
		{
			Debug.Log("Jump Action!");
			rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
		}
	}
    private void LeaveHide(LeaveStealthEvent e)
    {
		isHiding = false;
		// TODO: Add animation
    }

    private void EnterHide(EnterStealthEvent e)
    {
		isHiding = true;
		// TODO: Add animation
    }
	
	
}
