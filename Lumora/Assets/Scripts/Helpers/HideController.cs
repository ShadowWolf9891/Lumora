using System;
using System.Collections.Generic;
using UnityEngine;

public class HideController : MonoBehaviour
{
	[Header("Stealth Variables")]
	public LayerMask coverLayerMask;

	//Get all the colliders within a sphere
	private readonly Collider[] buffer = new Collider[16];

	/// <summary>
	/// Get the closest wall within a set radius from a location. Uses cover layer mask.
	/// </summary>
	/// <param name="sourceLocation">Location to spawn overlap sphere and check closest distance from.</param>
	/// <param name="radius">How big of a range to check for colliders.</param>
	/// <returns>The closest collider within the layer mask.</returns>
	public Collider GetClosestWall(Vector3 sourceLocation, float radius = 2f)
	{
		int count = Physics.OverlapSphereNonAlloc(sourceLocation,radius, buffer, coverLayerMask);

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
		return closestWall;
	}
}
