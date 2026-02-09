using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[SerializeField] GameObject playerRef;
	[SerializeField] SpawnableObjects spawnableObjects;
	[SerializeField] LayerMask coverLayerMask;

	List<GameObject> _cachedObjects = new();
	private void Awake()
	{
		SpawnerManager.Load(spawnableObjects);
		TimelineManager.Load();
	}
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		GameEvents<ToggleVisibilityEvent>.Subscribe(ToggleVisibility);
        Cursor.lockState = CursorLockMode.Locked;

		switch (SceneManager.GetActiveScene().buildIndex)
		{
			case 1:
				EventManager.Raise("ShiftLeader_Enter");
				break;
			case 2:
				EventManager.Raise("Chapter2_Intro");
				break;
			default:
				break;
		}

		//EventManager.Raise("UnlockThrow");
		//EventDispatcher.DispatchForCurrentQuest("SubQuest1");
	}

	private void Update()
	{
		EventManager.HandleEvents(); //Handle events each frame if there are any.
	}

	/// <summary>
	/// Toggle the visibility of a specific object by its name when event is triggered.
	/// </summary>
	/// <param name="e"></param>
	private void ToggleVisibility(ToggleVisibilityEvent e)
	{
		GameObject curObject = _cachedObjects.Find(x => x.name == e.ObjectName);
        if (curObject == null)
        {
			curObject = GameObject.Find(e.ObjectName);
			_cachedObjects.Add(curObject);
        }

        if(curObject != null) curObject.SetActive(e.IsVisible);
		else Debug.LogError($"Failed to find object {e.ObjectName} when calling {e.Id}.");
	
	}
}
