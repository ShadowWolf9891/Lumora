using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HideController
{
	public static LayerMask CoverLayerMask { get; private set; }

	private static float _attachRange = 1f; //How far away to begin attaching to the wall
	private static float _snapDistance = 0.6f; //How far away to snap the player from the wall

	//Get all the colliders within a sphere
	private static readonly Collider[] buffer = new Collider[16];

	public static void Load(LayerMask coverLayerMask, float attachRange = 1f, float snapDistance = 0.6f)
	{
		CoverLayerMask = coverLayerMask;
		_attachRange = attachRange;
		_snapDistance = snapDistance;
	}

	/// <summary>
	/// Change the object's movement if they are hiding so that they move along the wall.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="moveIntent"></param>
	/// <returns>The direction of movement.</returns>
	public static Vector3 GetHideMovement(Vector3 position, Vector3 moveIntent, Collider wall)
	{
		//If there is no closest wall, the intended movement is unmodified.
		if (wall == null) return moveIntent;

		Vector3 wallPoint = wall.ClosestPoint(position);
		Vector3 wallNormal = position - wallPoint;
		wallNormal.y = 0f;
		wallNormal.Normalize();

		// Slide along wall
		return Vector3.ProjectOnPlane(moveIntent, wallNormal);
	}
	/// <summary>
	/// Get the closest wall within a set radius from a location. Uses cover layer mask.
	/// </summary>
	/// <param name="sourceLocation">Location to spawn overlap sphere and check closest distance from.</param>
	/// <param name="radius">How big of a range to check for colliders.</param>
	/// <returns></returns>
	public static Collider GetClosestWall(Vector3 sourceLocation, float radius = 2f)
	{
		int count = Physics.OverlapSphereNonAlloc(sourceLocation,radius,buffer,CoverLayerMask);

		if (count == 0) return null; //Return if nothing hit
		
		float closestDistance = float.MaxValue;
		Collider closestWall = null;

		for (int i = 0; i < count; i++)
		{
			Collider c = buffer[i];
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
			return null;
		}
		return closestWall;
	}

	/// <summary>
	/// Snap an object to target wall. Use a raycast to make sure it is viable and consistent.
	/// </summary>
	/// <param name="position">Position of the object</param>
	/// <param name="wall">Wall to snap to.</param>
	/// <param name="offset">How far from the wall to snap to.</param>
	/// <returns>The new position of the object.</returns>
	public static Vector3 SnapToWall(Vector3 position, Collider wall, float offset = 0.6f)
	{
		if (wall == null) return position;

		Vector3 directionToWall = (wall.bounds.center - position).normalized;

		Ray ray = new Ray(position, directionToWall);

		if (wall.Raycast(ray, out RaycastHit hit, 2f))
		{
			Vector3 snapPos = hit.point + hit.normal * offset;
			snapPos.y = position.y;
			return snapPos;
		}

		return position;
	}

}
