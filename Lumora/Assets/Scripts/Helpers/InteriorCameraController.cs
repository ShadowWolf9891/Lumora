using UnityEngine;

public class InteriorCameraController : MonoBehaviour
{
	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && CameraManager.Instance.CurrentCamera.gameObject.name != "InteriorCamera")
		{
			if (GameManager.Instance.CurrentGameState == GameStates.Running)
			{
				CameraManager.Instance.SetCurrentCamera("InteriorCamera");
			}
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && CameraManager.Instance.CurrentCamera.gameObject.name == "InteriorCamera")
		{
			if (GameManager.Instance.CurrentGameState == GameStates.Running)
			{
				CameraManager.Instance.SetCurrentCamera("3rd Person Camera");
			}
		}
	}
}
