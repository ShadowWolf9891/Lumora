using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[SerializeField] GameObject playerRef;
	[SerializeField] SpawnableObjects spawnableObjects;
	[SerializeField] LayerMask coverLayerMask;

	private void Awake()
	{
		SpawnerManager.Load(spawnableObjects);
		TimelineManager.Load();
	}
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

		switch (SceneManager.GetActiveScene().buildIndex)
		{
			case 1:
				EventManager.Raise("ShiftLeader_Enter");
				break;
			case 2:
				EventManager.Raise("KipEnterHouse_C1_S2");
				break;
			case 3:
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
}
