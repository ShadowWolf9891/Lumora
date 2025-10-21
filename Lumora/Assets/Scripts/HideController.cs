using System;
using System.Collections.Generic;
using UnityEngine;

public class HideController : MonoBehaviour
{
	[Header("Stealth Variables")]
	public  LayerMask coverLayerMask;
	
	List<Collider> nearbyWalls = new();
	public Collider GetClosestCollider(Vector3 sourceLocation)
	{
		if (nearbyWalls == null || nearbyWalls.Count == 0)
		{
			Debug.Log("No nearby walls to check.");
			return null;
		}

		float closestDistance = float.MaxValue;
		Collider tempObject = null;

		foreach (Collider c in nearbyWalls)
		{
			float tempDistance = Vector3.Distance(c.ClosestPoint(sourceLocation), sourceLocation);

			if (tempDistance < closestDistance)
			{
				closestDistance = tempDistance;
				tempObject = c;
			}
		}

		if (tempObject == null)
			Debug.LogWarning("No valid wall found with line of sight.");

		return tempObject;
	}

	void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & coverLayerMask) != 0)
		{
			if (!nearbyWalls.Contains(other))
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
			Debug.Log($"{other.name} left trigger");
		}
		
	}
}
