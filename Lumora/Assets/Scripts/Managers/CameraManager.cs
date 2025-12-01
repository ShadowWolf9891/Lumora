using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public static class CameraManager
{
	static List<CinemachineCamera> _cameraList;
	static CinemachineBrain _brain;

	public static CinemachineCamera CurrentCamera { get; private set; }
	public static CinemachineCamera PreviousCamera { get; private set; }

	/// <summary>
	/// Load all of the cinemachine cameras in the scene into the list. 
	/// No need to call this unless you add your own cameras during runtime.
	/// </summary>
	private static void LoadCameras()
	{
		_cameraList = GameObject.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.InstanceID).ToList();
		_brain = CinemachineBrain.GetActiveBrain(0);
		if(CurrentCamera ==null)
		{
			SetCurrentCamera("3rd Person Camera");
		}
	}

	/// <summary>
	/// Set the current cinemachine camera based on its index, ordered by instance ID.
	/// </summary>
	/// <param name="cameraIndex"></param>
    public static void SetCurrentCamera(int cameraIndex, float blendSpeed = 1.0f)
    {
		if(_cameraList == null) { LoadCameras(); }

		if(cameraIndex >= _cameraList.Count) 
		{
			Debug.LogWarning($"Cannot set camera to cameraIndex: {cameraIndex}. IndexOutOfRangeException.");
			return;
		}

		if (CurrentCamera == _cameraList[cameraIndex]) return;

		PreviousCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
		CurrentCamera = _cameraList[cameraIndex];

		PreviousCamera.gameObject.SetActive(false);
		CurrentCamera.gameObject.SetActive(true);

		_brain.DefaultBlend.Time = blendSpeed;

		PreviousCamera.Priority = 0;
		CurrentCamera.Priority = 10;
    }
	/// <summary>
	/// Set the current camera based on the cinemachine camera name.
	/// </summary>
	/// <param name="cameraName"></param>
	public static void SetCurrentCamera(string cameraName, float blendSpeed = 1.0f)
	{
		if (_cameraList == null) { LoadCameras(); }
		int index = _cameraList.FindIndex(cam => cam.name == cameraName);
		if (index > -1)
		{
			SetCurrentCamera(index,blendSpeed);
		}
		else
		{
			Debug.LogWarning($"Cannot set camera to cameraIndex: {index}. IndexOutOfRangeException. {_cameraList.Count}");
		}
	}
	/// <summary>
	/// Set the current active camera to the previous camera.
	/// </summary>
	/// <param name="blendSpeed"></param>
	public static void ReturnToPreviousCamera(float blendSpeed = 1.0f)
	{
		if (PreviousCamera == null || CurrentCamera == null) 
		{
			Debug.LogWarning($"No previous camera to return to.");
			return;
		}

		(PreviousCamera, CurrentCamera) = (CurrentCamera, PreviousCamera);
		PreviousCamera.Priority = 0;
		CurrentCamera.Priority = 10;

		PreviousCamera.gameObject.SetActive(false);
		CurrentCamera.gameObject.SetActive(true);

		_brain.DefaultBlend.Time = blendSpeed;
	}

	public static bool IsBlending()
	{
		if(_brain == null) return false;
		return _brain.IsBlending;
	}
}
