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

		EventManager.Raise("Chapter1_Intro");
		Debug.Log($"Frame {Time.frameCount} QueueCount={EventManager.EventQueue.Count}");
		//EventManager.Raise("Chapter2_Intro");
		//EventManager.Raise("UnlockThrow");
		//EventDispatcher.DispatchForCurrentQuest("SubQuest1");
	}

	private void Update()
	{
		EventManager.HandleEvents(); //Handle events each frame if there are any.
	}
}
