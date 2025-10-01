using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
	[Header("Player Stats")]
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
	
	//Private properties
	bool isHiding;
	Rigidbody rb;
	private GameObject coverObject;

	[Header("Awareness")]
	[SerializeField, UnityEngine.Range(4,32), Tooltip("The number of directions to send a raycast out. Higher numbers make it less likely to miss, but cost more performance.")]
	private int directionsToCheck;
	
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		GameContext.Instance.OnMove += Move;
        GameContext.Instance.OnAttackPressed += Attack;
        GameContext.Instance.OnInteractPressed += Interact;
		GameContext.Instance.OnHidePressed += DoHide;
		GameContext.Instance.OnJumpPressed += Jump;
		GameContext.Instance.OnPlayerSpotted += GetSpotted;


		rb = GetComponent<Rigidbody>();
    }
	private void Move(Vector3 moveDirection)
	{
		//movement
		if (moveDirection != Vector3.zero && !isHiding)
		{
			rb.AddForce(acceleration * Time.deltaTime * 60 * moveDirection, ForceMode.Acceleration);
		}
		//Movement behind cover
		else if (moveDirection != Vector3.zero && isHiding && coverObject != null)
		{
			Collider currentCollider = coverObject.GetComponent<Collider>();
			Physics.Raycast(transform.position, (currentCollider.ClosestPoint(transform.position) - transform.position).normalized,
			out RaycastHit currentHit);

			Vector3 currentNormal = currentHit.normal;
			Vector3 currentPoint = currentHit.point;

			// Project move direction onto current plane
			Vector3 projected = Vector3.ProjectOnPlane(moveDirection, currentNormal).normalized;

			if (Physics.Raycast(transform.position, moveDirection.normalized, out RaycastHit forwardHit, 1f, ~0))
			{
				// If wall found and it's not the same object
				if (forwardHit.collider.gameObject != coverObject)
				{
					// Switch cover!
					coverObject = forwardHit.collider.gameObject;
					currentNormal = forwardHit.normal;
					currentPoint = forwardHit.point;

					projected = Vector3.ProjectOnPlane(moveDirection, currentNormal).normalized;
				}
			}
			float distanceToCover = Vector3.Distance(transform.position, currentPoint);
			if (distanceToCover > 1f)
			{
				coverObject = null;
				TryHide();
				return;
			}
			else
			{
				// Snap to correct distance from wall
				Vector3 offset = currentNormal.normalized * (0.6f - distanceToCover);
				rb.MovePosition(Vector3.Slerp(transform.position, transform.position + offset, 0.5f));

				// Then apply movement along the wall
				rb.AddForce(acceleration * Time.deltaTime * 60 * projected, ForceMode.Acceleration);
			}
			Debug.DrawLine(transform.position, transform.position + projected, Color.green);
		}

		FaceMoveDirection(moveDirection);
		//adding drag while grounded
		if (IsGrounded() && rb.linearVelocity.magnitude > 0.1f && !isHiding)
		{
			rb.AddForce(-stoppingForce * Time.deltaTime * 60 * transform.forward, ForceMode.Acceleration);
			//Debug.Log($"Running Stopping force, dragForce = {dragForce.x}, {dragForce.z}");
		}
		HandleSpeedControl();
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
		Vector3 groundSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
		if (groundSpeed.magnitude > maxSpeed)
		{
			Vector3 limitedVelocity = groundSpeed.normalized * maxSpeed;
			rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
		}
	}
	private void Attack()
	{
		Debug.Log("Attack Pressed.");
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
		coverObject = GetClosestObject(1, ~0);

		if (coverObject != null)
        {
            //Toggle hiding
            isHiding = true;
            GameContext.Instance.RaiseEnterStealth();
        }
        else
		{
			isHiding = false;
			GameContext.Instance.RaiseLeaveStealth();
		}
	}

	/// <summary>
	/// Triggers on button press. Player tries to hide if not hiding, untoggles hiding state when hiding.
	/// </summary>
    private void DoHide()
    {
        coverObject = GetClosestObject(1, ~0);

        if (!isHiding)
        {
			TryHide();
        }
        else
        {
			isHiding = false;
			GameContext.Instance.RaiseLeaveStealth();
        }
    }

	/// <summary>
	/// behavior for when player is spotted. Runs via gamecontext event
	/// </summary>
	private void GetSpotted()
    {
        isHiding = false;
        GameContext.Instance.RaiseLeaveStealth();
		//give player temporary movespeed buff? players should run away here, right?
    }

    /// <summary>
    /// Check the surroundings of the player by casting rays in a number of directions. 
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="hitLayer"></param>
    /// <returns>A list of colliders that were hit on the layer <paramref name="hitLayer"/> or a new empty list.</returns>
    private List<Collider> CheckSurroundings(float distance, LayerMask hitLayer)
	{
		List<Collider> hitObjects = new();
		for(int i = 0; i < directionsToCheck; i++) 
		{
			// Angle in degrees
			float angle = i * (360f / directionsToCheck);

			// Rotate forward vector around Y
			Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
			Debug.DrawRay(transform.position, direction * distance, Color.red, 1f);
			if(Physics.Raycast(transform.position, direction, out RaycastHit rHit, distance,  hitLayer))
			{
				if (!hitObjects.Contains(rHit.collider))
				{
					hitObjects.Add(rHit.collider);
				}
			}
			//Send out another burst at the player's feet
			if (Physics.Raycast(transform.position - new Vector3(0,1,0), direction, out RaycastHit rHit2, distance, hitLayer))
			{
				if (!hitObjects.Contains(rHit2.collider))
				{
					hitObjects.Add(rHit2.collider);
				}
			}
		}

		return hitObjects.Count > 0 ? hitObjects: new();
	}
	public GameObject GetClosestObject(float distance, int layerMask = ~0)
	{
		
		List<Collider> nearbyWalls = CheckSurroundings(distance, layerMask);

		if (nearbyWalls.Count == 0 || nearbyWalls == null) return null; //Return if nothing was hit

		float closestDistance = float.MaxValue;
		GameObject tempObject = null;
		//Find the closest object to the player and set it as the cover object
		foreach (Collider c in nearbyWalls)
		{
			float tempDistance = Vector3.Distance(c.ClosestPoint(transform.position), transform.position);
			if (tempDistance < closestDistance)
			{
				closestDistance = tempDistance;
				tempObject = c.gameObject;
			}
		}
		return tempObject;
	}

	private void Jump()
	{
		if (IsGrounded())
		{
			Debug.Log("Jump Action!");
			rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
		}
	}
}
