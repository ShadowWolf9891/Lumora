using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(HideController))]
public class PlayerBehavior : MonoBehaviour, ISaveable
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
	[SerializeField] Vector3 startVelocity;
	//line renderer 
	[SerializeField] LineRenderer lineRenderer;
	[SerializeField] GameObject hitSpherePrefab;
	private GameObject activeHitSphere;
	private int linePoints = 16;
	private float timeBetweenPoints = 0.15f;
	private bool isThrowing;
	private bool canThrow;

	[Header("Stealth Settings")]
	[SerializeField] private float detectDistance = 1f;
	[SerializeField] private float stealthSpeedModifier = 0.5f;
    [SerializeField] private float sprintNoiseMade = 5f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchedHeight = 1.4f;
	[SerializeField] private float stealthSnapDistance = 0.6f;
	private Collider coverObject;

	[Header("WaypointSettings")]
	[SerializeField] private GameObject waypointImage;

	//State settings
	[HideInInspector]public bool IsCrouching { get; private set; }
	[HideInInspector] public bool IsSprinting { get; private set; }

	//Private properties
	private HideController hideController;
	private PlayerHealthBehaviors playerHealthBehaviors;
	bool isHiding = false;
	Rigidbody rb;
	CapsuleCollider playerCollider;
	PathObjectBehavior pathObjectBehavior;
	private Vector3 lastWallNormal = Vector3.zero;
	private Camera mainCam;
	private Vector3 curThrowDirection;
	private float throwYaw;
	private float throwPitch;
	#endregion

	#region Initializing
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Awake()
    {
        SubscribeToEvents();
        GetComponentReferences();
    }
	private void Start()
	{

		CameraManager.Instance.SetCurrentCamera("3rd Person Camera");
	}

	private void SubscribeToEvents()
    {
        GameEvents<PlayerInputEvent>.Subscribe(HandleInput);
        GameEvents<PlayerSpottedEvent>.Subscribe(GetSpotted);
        GameEvents<EnterStealthEvent>.Subscribe(EnterHide);
        GameEvents<LeaveStealthEvent>.Subscribe(LeaveHide);
		GameEvents<UnlockAbilityEvent>.Subscribe(UnlockAbility);

    }

    private void GetComponentReferences()
    {
        rb = GetComponent<Rigidbody>();
        hideController = GetComponent<HideController>();
        playerCollider = GetComponent<CapsuleCollider>();
		mainCam = Camera.main;
		pathObjectBehavior = TryGetComponent(out PathObjectBehavior pathObj) ? pathObj : null;
		playerHealthBehaviors = TryGetComponent(out PlayerHealthBehaviors pHealth) ? pHealth : null;
	}

    void OnDestroy()
    {
        GameEvents<PlayerInputEvent>.Unsubscribe(HandleInput);
        GameEvents<PlayerSpottedEvent>.Unsubscribe(GetSpotted);
        GameEvents<EnterStealthEvent>.Unsubscribe(EnterHide);
        GameEvents<LeaveStealthEvent>.Unsubscribe(LeaveHide);
		GameEvents<UnlockAbilityEvent>.Unsubscribe(UnlockAbility);
    }
    #endregion

    #region Handle Input
    private void Update()
	{
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
	private void Crouch()
	{
		IsCrouching = !IsCrouching;
		playerCollider.height = IsCrouching ? crouchedHeight : standingHeight;
		playerCollider.center = IsCrouching ? new Vector3(0, crouchedHeight / 2, 0) : new Vector3(0,standingHeight / 2,0);
	}
	private void Jump()
	{
		if (IsGrounded())
		{
			rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
		}
	}
	private void Interact()
	{
		//If something to interact with

		//else

		//Stop hiding if you were hiding previously
		if (isHiding)
		{
			coverObject = null;
			GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
		}
		else
		{
			//If not hiding, get the closest object. If there is one within range, enter stealth.
			Collider closest = hideController.GetClosestWall(transform.position);
			coverObject = closest ? closest : null;
			if (coverObject != null)
			{
				//Toggle hiding
				GameEvents<EnterStealthEvent>.Raise(new EnterStealthEvent("enter_Stealth"));
			}
		}
	}
	//Contains basic movement, crouched movement, jumping, and all helpers associated
	#region Movement
	private void Move(Vector3 moveDirection)
	{
		HandleWaypoints();
		if (isHiding) HideMove(moveDirection);
		else
        {
			rb.AddForce(acceleration * Time.fixedDeltaTime * 60 * moveDirection, ForceMode.Acceleration);
            FaceMoveDirection(moveDirection);
        }

        if (isThrowing) { UpdateThrow(Vector3.zero); }
	}
    private void DoSprint()
    {
		//Called via HandleInput(). Starts player sprinting that continues until player stops moving.
		if (IsCrouching)
		{
			GameEvents<PlayerInputEvent>.Raise(new PlayerInputEvent("crouch", PlayerInputActionType.Crouch, true));
		}
		if(isHiding) 
		{
			GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
		}

		if (!IsSprinting)
		{
			IsSprinting = true;
		}
		else IsSprinting = false;
    }
	public void TriggerSprintNoise()
    {
        GameEvents<SpawnVisibleNoiseEvent>.Raise(new SpawnVisibleNoiseEvent("VisibleNoise", true, transform.position, sprintNoiseMade));
	}
    private void HideMove(Vector3 moveDirection)
	{
		Vector3 nextPosition = transform.position + moveDirection;
		if (coverObject == null) return;

		Collider currentCollider = hideController.GetClosestWall(transform.position);
		if (currentCollider == null) return;

		// Get wall contact point and normal
		Vector3 wallPoint = currentCollider.ClosestPoint(transform.position);
		Vector3 wallNormal = (transform.position - wallPoint).normalized;

		if (wallNormal.sqrMagnitude > 0.0001f)
			lastWallNormal = wallNormal; // Cache for stability

		Vector3 projectedNextPosition = nextPosition - lastWallNormal * Vector3.Dot(nextPosition - wallPoint, lastWallNormal);

		Vector3 movementAlongPlane = (projectedNextPosition - transform.position);
		rb.AddForce(acceleration * Time.fixedDeltaTime * 60 * stealthSpeedModifier * movementAlongPlane, ForceMode.Acceleration);

		float distanceToWall = Vector3.Dot(transform.position - wallPoint, lastWallNormal);
		if (Mathf.Abs(distanceToWall - stealthSnapDistance) > 0.01f)
		{
			Vector3 snapTarget = transform.position - lastWallNormal * (distanceToWall - stealthSnapDistance);
			rb.MovePosition(Vector3.Lerp(transform.position, snapTarget, 0.5f));
		}

		FaceMoveDirection(moveDirection);
		//Debug for crouch movement, uncomment to re-enable.
		Debug.DrawLine(transform.position, projectedNextPosition, UnityEngine.Color.green);
		Debug.DrawLine(transform.position, currentCollider.ClosestPoint(transform.position), UnityEngine.Color.red);
		Debug.DrawLine(nextPosition, currentCollider.ClosestPoint(nextPosition), UnityEngine.Color.red);
		Debug.DrawLine(currentCollider.ClosestPoint(transform.position), currentCollider.ClosestPoint(nextPosition), UnityEngine.Color.orange);
		Debug.DrawLine(transform.position, transform.position + wallNormal, UnityEngine.Color.blue);
	}

	#endregion

	#endregion

	#region Stealth Events
	/// <summary>
	/// Behavior for when player is spotted. Runs via gamecontext event
	/// </summary>
	private void GetSpotted(PlayerSpottedEvent e)
    {
		GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth"));
    }
    private void EnterHide(EnterStealthEvent e)
    {
        IsSprinting = false;
        isHiding = true;
		coverObject = hideController.GetClosestWall(transform.position);
		if (coverObject == null) { GameEvents<LeaveStealthEvent>.Raise(new LeaveStealthEvent("leave_Stealth")); }
	}
	private void LeaveHide(LeaveStealthEvent e)
	{
		isHiding = false;
		coverObject = null;
		lastWallNormal = Vector3.zero;
	}
	#endregion

	#region Throwing
	private void PrepareThrow()
	{
		//when pressing throw key, creates a line render to show expected trajectory for projectile
		//Debug.Log("Preparing throw.");
		isThrowing = true;
		
		CameraManager.Instance.SetCurrentCamera("ThrowCamera", 0.2f);
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
		if (!CameraManager.Instance.IsBlending())
		{
			GameObject projectile = Instantiate(thrownObjPrefab, throwLocation.position, Quaternion.identity);
			Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
			projectileRb.AddForce(startVelocity, ForceMode.Impulse);
		}
		CameraManager.Instance.ReturnToPreviousCamera(0.5f);
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

	#region Helpers
	/// <summary>
	/// Checks if the player is on the ground or not.
	/// </summary>
	/// <returns></returns>
	private bool IsGrounded()
	{
		Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - playerHeight / 2, transform.position.z), UnityEngine.Color.darkRed);
		return Physics.Raycast(transform.position, Vector3.down, playerHeight / 2, groundedLayers) && rb.linearVelocity.y <= Mathf.Abs(0.001f);
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

	private void HandleWaypoints()
	{
		if (pathObjectBehavior == null) return;
		if (waypointImage.transform.position == Vector3.zero) waypointImage.transform.position = pathObjectBehavior.RestartPath();

		if (pathObjectBehavior.IsDonePath(transform.position, 5f))
		{
			GameEvents<ToggleVisibilityEvent>.Raise(new ToggleVisibilityEvent("hideWaypoint", waypointImage.name, false));
		}
		else if(!waypointImage.activeInHierarchy) 
		{
			GameEvents<ToggleVisibilityEvent>.Raise(new ToggleVisibilityEvent("showWaypoint", waypointImage.name, true));
		}

		if(pathObjectBehavior.IsAtPoint(transform.position, 5f))
		{
			waypointImage.transform.position = pathObjectBehavior.GetNextPoint();
		}

	}
	#endregion

	#region Save/Load
	public void Save(GameSaveData data)
	{
		data.playerData = new PlayerSaveData()
		{
			position = new SerializableVector3(transform.position),
			rotation = new SerializableVector3(transform.rotation.eulerAngles),
			health = playerHealthBehaviors.CurrentHealthValue,
			pathData = new PathData() { 
				CurrentPath = pathObjectBehavior.GetCurrentPathAndPoint().Item1,
				CurrentPoint = pathObjectBehavior.GetCurrentPathAndPoint().Item2 },
			inventory = InventoryManager.Instance.InventoryData
		};
	}
	public void Load(GameSaveData data)
	{
		if (data == null) return;
		if(data.playerData.position.ToVector3() != Vector3.zero) transform.SetPositionAndRotation(data.playerData.position.ToVector3(), Quaternion.Euler(data.playerData.rotation.ToVector3()));
		playerHealthBehaviors.CurrentHealthValue = data.playerData.health;
		if(data.playerData.pathData.CurrentPath != -1 && data.playerData.pathData.CurrentPoint != -1)
			waypointImage.transform.position = pathObjectBehavior.GoToPath(data.playerData.pathData.CurrentPath, data.playerData.pathData.CurrentPoint);
		InventoryManager.Instance.InventoryData = data.playerData.inventory;
		CameraManager.Instance.SetCurrentCamera("3rd Person Camera", 0f);
	}
	
	#endregion
}
