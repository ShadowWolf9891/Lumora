using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HideController : MonoBehaviour
{
	[Header("Stealth Variables")]
	public LayerMask coverLayerMask;
	[SerializeField] private string tagToIgnore;
	[SerializeField] private float attachThreshold = 1f; //Dot product threshold to be considered attaching
	[SerializeField] private float detachThreshold = 0.2f; //Dot product threshold to consider as detaching
	[SerializeField] private float detachRate = 1f; //How quickly the player should detach as a multiplier
	[SerializeField] private float decayRate = 1f; //How quickly the players attempts to detach from a wall fall off
	[SerializeField] private float attachRange = 1f; //How far away to begin attaching to the wall
	[SerializeField] private float snapDistance = 0.6f; //How far away to snap the player from the wall
	[HideInInspector] public bool IsInCover = false;
	[HideInInspector] public Vector3 CurrentMoveIntent { get; private set; }

	private float detachCharge = 0f;
	private float attachCharge = 0f;
	private Coroutine detachRoutine, attachRoutine, snapRoutine;

	private Vector3 wallNormal = Vector3.zero;
	private Vector3 lastMoveInput = Vector3.zero;
	private Vector3 snapMovementModifier = Vector3.zero;
	List<Collider> nearbyWalls = new();
	Collider closestWall;

	CoverState curState = CoverState.Free;
	enum CoverState
	{
		Free,
		Snapping,
		InCover,
		Detaching
	}
	
	#region Handle Movement States
	public Vector3 ResolveMovement(Vector3 moveIntent, Vector3 position)
	{
		SetMoveIntent(moveIntent);
		GetClosestWall(position);
		Debug.DrawRay(transform.position, wallNormal,
		curState == CoverState.InCover ? Color.blue :
		curState == CoverState.Snapping ? Color.yellow :
		Color.green);

		Vector3 horizontalMovement = Vector3.zero;

		switch (curState)
		{
			case CoverState.Free:
				horizontalMovement = HandleFree(moveIntent, position);
				break;

			case CoverState.Snapping:
				horizontalMovement = HandleSnapping(moveIntent, position);
				break;

			case CoverState.InCover:
				horizontalMovement = HandleInCover(moveIntent, position);
				break;

			//case CoverState.Detaching:
				//return HandleDetaching(moveIntent, position);
		}
		
		horizontalMovement.y = 0;
		return horizontalMovement;
	}

	
	private Vector3 HandleFree(Vector3 moveIntent, Vector3 position)
	{
		if(closestWall == null) return moveIntent;

		float dist = DistanceToWall(position);
		if (dist <= attachRange && IsPushingInto())
		{
			curState = CoverState.Snapping;
		}
		return moveIntent;
	}
	private Vector3 HandleSnapping(Vector3 moveIntent, Vector3 position)
	{
		if (IsPullingAway())
		{
			curState = CoverState.Free;
			return moveIntent;
		}

		float dist = DistanceToWall(position);

		if (Mathf.Abs(dist - snapDistance) < 0.05f)
		{
			curState = CoverState.InCover;
			return moveIntent;
		}

		Vector3 wallPoint = closestWall.ClosestPoint(position);
		Vector3 snapDir = wallPoint - position;
		snapDir.y = 0f;
		return snapDir.normalized;
	}
	private Vector3 HandleInCover(Vector3 moveIntent, Vector3 position)
	{
		if(IsPullingAway())
		{
			curState = CoverState.Free;
			return moveIntent;
		}

		Vector3 planarNormal = wallNormal;
		planarNormal.y = 0f;

		// Project movement along wall plane
		return moveIntent - planarNormal * Vector3.Dot(moveIntent, planarNormal);
	}
	//private Vector3 HandleDetaching(Vector3 moveIntent, Vector3 position)
	//{

	//}
	#endregion
	#region Wall Detection
	public void GetClosestWall(Vector3 sourceLocation)
	{
		closestWall = null;
		if (nearbyWalls == null || nearbyWalls.Count == 0)
		{
			return;
		}

		float closestDistance = float.MaxValue;
		foreach (Collider c in nearbyWalls)
		{
			//Debug.Log($"Running Find Distance on {c.gameObject.name}");
			float tempDistance = Vector3.Distance(c.ClosestPoint(sourceLocation), sourceLocation);

			if (tempDistance < closestDistance)
			{
				closestDistance = tempDistance;
				closestWall = c;
			}
		}

		if (closestWall == null)
		{
			Debug.LogWarning("No valid wall found with line of sight.");
			return;
		}

		Vector3 wallPoint = closestWall.ClosestPoint(sourceLocation);
		wallNormal = (sourceLocation - wallPoint);
		wallNormal.y = 0;
		wallNormal.Normalize();
	}
	#endregion
	#region Helpers
	private bool IsPullingAway()
	{
		return Vector3.Dot(CurrentMoveIntent, wallNormal) > detachThreshold;
	}
	bool IsPushingInto()
	{
		return Vector3.Dot(CurrentMoveIntent, wallNormal) < -0.2f;
	}
	private float DistanceToWall(Vector3 position)
	{
		if(closestWall == null) return float.MaxValue;

		return Vector3.Distance(transform.position, closestWall.ClosestPoint(transform.position));
	}
	public void SetMoveIntent(Vector3 moveDirection)
	{
		Vector3 groundMovement = new Vector3(moveDirection.x, 0, moveDirection.z);
		CurrentMoveIntent = groundMovement.sqrMagnitude > 0.0001f
			? groundMovement.normalized
			: Vector3.zero;
	}
	#endregion
	#region Colliders
	void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & coverLayerMask) != 0)
		{
			if (!nearbyWalls.Contains(other) && !other.gameObject.CompareTag("NoiseTag"))
			{
				nearbyWalls.Add(other);
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (nearbyWalls.Contains(other))
		{
			nearbyWalls.Remove(other);
		}
		
	}
	#endregion
}
