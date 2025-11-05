using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager
{
	static List<CinemachineCamera> _cameraList;
	static CinemachineBrain _brain;

	public static CinemachineCamera CurrentCamera { get; private set; }
	public static CinemachineCamera PreviousCamera { get; private set; }

	private static Queue<GameEventType> inProgressEvents = new Queue<GameEventType>();

	/// <summary>
	/// Setup the camera manager. Make sure to call this during awake to correctly subscribe to events.
	/// </summary>
	public static void Load()
	{
		GameEvents<CameraMoveEvent>.Subscribe(MoveCinematicCamera);
		GameEvents<CameraPanEvent>.Subscribe(PanCinematicCamera);
	}
	/// <summary>
	/// Load all of the cinemachine cameras in the scene into the list. 
	/// No need to call this unless you add your own cameras during runtime.
	/// </summary>
	private static void LoadCameras()
	{
		_cameraList = GameObject.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.InstanceID).ToList();
		_brain = CinemachineBrain.GetActiveBrain(0);
		if(CurrentCamera ==null) CurrentCamera = _cameraList.Last();
	}

	public static void UpdateCameraEvents()
	{
		if (_cameraList == null) { LoadCameras(); }
		if (CurrentCamera.name == "Cinematic_Camera")
		{
			if (inProgressEvents.Count > 0)
			{
				GameEventType currentEvent = inProgressEvents.Peek();
				if (currentEvent is CameraMoveEvent cameraEvent)
				{
					CurrentCamera.transform.position = Vector3.Slerp(CurrentCamera.transform.position, cameraEvent.WorldLocation + cameraEvent.TargetLocation, cameraEvent.MoveSpeed);

					if (CurrentCamera.transform.position.magnitude - (cameraEvent.WorldLocation + cameraEvent.TargetLocation).magnitude <= 0.1f)
					{
						EventManager.MarkEventCompleted(currentEvent.Id);
						inProgressEvents.Dequeue();
					}
				}
				if (currentEvent is CameraPanEvent cameraPanEvent)
				{
					Quaternion currentRot = CurrentCamera.transform.rotation;
					Quaternion targetRot = Quaternion.Euler(cameraPanEvent.WorldRotation + cameraPanEvent.TargetRotation);

					CurrentCamera.transform.rotation = Quaternion.Slerp(
						currentRot,
						targetRot,
						cameraPanEvent.RotationSpeed * Time.deltaTime
					);

					if (Quaternion.Angle(currentRot, targetRot) <= 0.5f)
					{
						EventManager.MarkEventCompleted(currentEvent.Id);
						inProgressEvents.Dequeue();
					}
				}
			}
			else
			{
				ReturnToPreviousCamera(0);
			}
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
			Debug.LogWarning($"Cannot set camera to cameraIndex: {index}. IndexOutOfRangeException.");
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

		_brain.DefaultBlend.Time = blendSpeed;
	}

	public static bool IsBlending()
	{
		if(_brain == null) return false;
		return _brain.IsBlending;
	}

	private static void MoveCinematicCamera(CameraMoveEvent e)
	{
		SetCurrentCamera("Cinematic_Camera", 0);
		CurrentCamera.transform.SetPositionAndRotation(e.WorldLocation, PreviousCamera.transform.rotation);
		inProgressEvents.Enqueue(e);

		if(e.AutoReturn) 
		{
			GameEvents<CameraMoveEvent>.Raise(new CameraMoveEvent(e.Id, - e.TargetLocation, e.WorldLocation + e.TargetLocation, e.MoveSpeed, false));
		}
	}

	private static void PanCinematicCamera(CameraPanEvent e)
	{
		SetCurrentCamera("Cinematic_Camera", 0);
		CurrentCamera.transform.SetPositionAndRotation(PreviousCamera.transform.position, Quaternion.Euler(e.WorldRotation));
		inProgressEvents.Enqueue(e);

		if (e.AutoReturn)
		{
			GameEvents<CameraPanEvent>.Raise(new CameraPanEvent(e.Id, -e.TargetRotation, e.WorldRotation + e.TargetRotation, e.RotationSpeed, false));
		}

		
	}
}
