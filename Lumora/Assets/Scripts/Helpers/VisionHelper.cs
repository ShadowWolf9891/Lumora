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
	public static bool CanSeeTarget(GameObject viewer, GameObject target, float viewAngle, float viewDistance, LayerMask layerMask)
	{
		//Make the target harder or easier to see if they have a visibility manager
		if(target.TryGetComponent<VisibilityManager>(out VisibilityManager vm))
		{
			viewDistance *= vm.Visibility;
		}

		Vector3 origin = viewer.transform.position + Vector3.up * 1.5f;
		Vector3 directionToTarget = target.transform.position - origin;
		float distanceToTarget = directionToTarget.magnitude;

		if (distanceToTarget > viewDistance)
			return false;

		float angleToTarget = Vector3.Angle(viewer.transform.forward, directionToTarget);
		if (angleToTarget > viewAngle * 0.5f) return false;

		Debug.DrawRay(origin, directionToTarget.normalized * (distanceToTarget), Color.mediumVioletRed, Time.deltaTime);
	
		if (Physics.Raycast(origin, directionToTarget.normalized, out RaycastHit hit, distanceToTarget, layerMask))
		{
			return hit.collider.gameObject == target || hit.collider.transform.IsChildOf(target.transform);
		}

		return false;
	}
}
