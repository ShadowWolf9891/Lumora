using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] GameObject playerRef;
	[SerializeField] SpawnableObjects spawnableObjects;

	private void Awake()
	{
        NPCManager.Load();
		SpawnerManager.Load(spawnableObjects);
		CameraManager.Load();

	}
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        EventManager.Raise("Chapter2_Intro");
		//EventDispatcher.DispatchForCurrentQuest("SubQuest1");

	}

    // Update is called once per frame
    void Update()
    {
		CameraManager.UpdateCameraEvents();
	}
}
