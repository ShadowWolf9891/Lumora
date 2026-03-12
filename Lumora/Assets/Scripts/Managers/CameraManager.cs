using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public static CameraManager Instance { get; private set; }
	[SerializeField] CinemachineBrain _brain;

	List<CinemachineCamera> _cameraList;
	public CinemachineCamera CurrentCamera { get; private set; }
	public CinemachineCamera PreviousCamera { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);
	}

	/// <summary>
	/// Load all of the cinemachine cameras in the scene into the list. 
	/// No need to call this unless you add your own cameras during runtime.
	/// </summary>
	public void LoadCameras()
	{
		_cameraList = GameObject.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.InstanceID).ToList();
		if(CurrentCamera ==null && _cameraList.Count > 0)
		{
			if (_cameraList.Exists(cam => cam.name == "3rd Person Camera")) SetCurrentCamera("3rd Person Camera");
			else SetCurrentCamera(0);
		}
	}

	/// <summary>
	/// Set the current cinemachine camera based on its index, ordered by instance ID.
	/// </summary>
	/// <param name="cameraIndex"></param>
    public void SetCurrentCamera(int cameraIndex, float blendSpeed = 1.0f)
    {
		if(_cameraList == null) { LoadCameras(); }

		if(cameraIndex >= _cameraList.Count) 
		{
			Debug.LogWarning($"Cannot set camera to cameraIndex: {cameraIndex}. IndexOutOfRangeException.");
			return;
		}

		if (CurrentCamera == _cameraList[cameraIndex]) return;

		if(_brain.ActiveVirtualCamera != null) PreviousCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
		CurrentCamera = _cameraList[cameraIndex];

		if(PreviousCamera) PreviousCamera.gameObject.SetActive(false);
		CurrentCamera.gameObject.SetActive(true);

		_brain.DefaultBlend.Time = blendSpeed;

		if(PreviousCamera) PreviousCamera.Priority = 0;
		CurrentCamera.Priority = 10;
    }
	/// <summary>
	/// Set the current camera based on the cinemachine camera name.
	/// </summary>
	/// <param name="cameraName"></param>
	public void SetCurrentCamera(string cameraName, float blendSpeed = 1.0f)
	{
		if (_cameraList == null) { LoadCameras(); }
		int index = _cameraList.FindIndex(cam => cam.name == cameraName);
		if (index > -1)
		{
			SetCurrentCamera(index,blendSpeed);
		}
		else
		{
			Debug.LogWarning($"Cannot set camera to cameraIndex: {index} with name {cameraName}. IndexOutOfRangeException. {_cameraList.Count}");
		}
	}
	/// <summary>
	/// Set the current active camera to the previous camera.
	/// </summary>
	/// <param name="blendSpeed"></param>
	public void ReturnToPreviousCamera(float blendSpeed = 1.0f)
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

	public bool IsBlending()
	{
		if(_brain == null) return false;
		return _brain.IsBlending;
	}

	public void Reset()
	{
		_cameraList = null;
		CurrentCamera = null;
		PreviousCamera = null;
	}
}
