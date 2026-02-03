using UnityEngine;

public static class VisionHelper
{
	/// <summary>
	/// Checks if the viewer can see the target.
	/// </summary>
	/// <param name="viewer">The observer GameObject.</param>
	/// <param name="target">The target GameObject.</param>
	/// <param name="viewAngle">FOV angle in degrees.</param>
	/// <param name="viewDistance">Maximum viewing distance.</param>
	/// <param name="layerMask">Layers to check for obstacles.</param>
	/// <returns>True if visible.</returns>
	public static bool CanSeeTarget(GameObject viewer, GameObject target, float viewDistance, float viewAngle, Vector3 viewOffset, LayerMask layerMask)
	{
		//Exit early if objects don't exist or if the target is out of the view angle.
		if (viewer == null || target == null) return false;
		

		//Make the target harder or easier to see if they have a visibility manager
		if (target.TryGetComponent<VisibilityManager>(out VisibilityManager vm))
		{
			viewDistance -= viewDistance * vm.Visibility;
		}

		//Generate the points where the viewer will try to see the target using it's bounds
		Bounds b = target.GetComponent<Collider>().bounds;
		Vector3 origin = viewer.transform.position + viewOffset;
		if (b.Contains(origin))return true;

		Vector3 closest = b.ClosestPoint(origin);
		Vector3 rightExtent = viewer.transform.right * GetExtentAlongDirection(b, viewer.transform.right);
		Vector3 upExtent = viewer.transform.up * GetExtentAlongDirection(b, viewer.transform.up);
		

		Vector3[] targetPoints =
		{
			closest,
			b.center,
			b.center + upExtent,
			b.center - upExtent,
			b.center + rightExtent,
			b.center - rightExtent
		};
		//Check if any points on the bounds are within the vision cone.
		bool insideView = false;
		foreach (var p in targetPoints)
		{
			Vector3 dir = p - viewer.transform.position;
			if (Vector3.Angle(viewer.transform.forward, dir) <= viewAngle *0.5f)
			{
				insideView = true;
				break;
			}
		}
		if (!insideView)
			return false;

		//Check if any rays hit the target collider.

		foreach (var point in targetPoints)
		{
			Vector3 direction = point - origin;
			float distance = direction.magnitude + 0.05f;

			if (distance > viewDistance) continue;
			Debug.DrawRay(origin, direction.normalized * (distance), Color.mediumVioletRed, Time.deltaTime);
			if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, layerMask))
			{
				if (hit.collider.transform.root == target.transform) return true;
			}

		}

		return false;
	}

	/// <summary>
	/// Gets the distance from the center of bounds to the outer edge along a direction.
	/// </summary>
	/// <param name="b">Bounds of target</param>
	/// <param name="direction">Which direction to check extent</param>
	/// <returns></returns>
	static float GetExtentAlongDirection(Bounds b, Vector3 direction)
	{
		direction.Normalize();

		Vector3 e = b.extents;
		return Mathf.Abs(direction.x) * e.x + Mathf.Abs(direction.y) * e.y + Mathf.Abs(direction.z) * e.z;
	}
}
