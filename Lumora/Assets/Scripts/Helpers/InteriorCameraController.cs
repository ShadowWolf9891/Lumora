using Unity.Cinemachine;
using UnityEngine;

public class InteriorCameraController : MonoBehaviour
{
	[SerializeField]float InsideRadius = 1.0f;
	[SerializeField] float OutsideRadius = 8.0f;
	bool isInside;

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !isInside)
		{
			if (GameManager.Instance.CurrentGameState == GameStates.Running)
			{
				CameraManager.Instance.CurrentCamera.GetComponent<CinemachineOrbitalFollow>().Radius = InsideRadius;
				isInside = true;
			}
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isInside)
		{
			if (GameManager.Instance.CurrentGameState == GameStates.Running)
			{
				CameraManager.Instance.CurrentCamera.GetComponent<CinemachineOrbitalFollow>().Radius = OutsideRadius;
				isInside = false;
			}
		}
	}
}
