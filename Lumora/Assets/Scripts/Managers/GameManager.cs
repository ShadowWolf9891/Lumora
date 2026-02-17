using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private GameManager Instance;
	[SerializeField] GameObject playerRef;
	[SerializeField] SpawnableObjects spawnableObjects;
	[SerializeField] LayerMask coverLayerMask;

	List<GameObject> _cachedObjects = new();

	public static GameStates CurrentGameState { get; private set; }

	private static GameSaveData _saveData;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!SaveSystem.HasSaved) SaveAll();
		SpawnerManager.Load(spawnableObjects);
		TimelineManager.Load();
		SceneLoader.LoadManager();

	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		GameEvents<ToggleVisibilityEvent>.Subscribe(ToggleVisibility);
		GameEvents<ChangeGameStateEvent>.Subscribe(OnGameStateChange);
		Cursor.lockState = CursorLockMode.Locked;

		LoadAll();
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

	public static void SaveAll()
	{
		_saveData ??= new GameSaveData();
		_saveData.eventData = new EventSaveData();
		_saveData.playerData = new PlayerSaveData();
		_saveData.worldData = new WorldSaveData();
		for(int i = 0; i < GetCache().Count; i++)
		{
			GetCache()[i].Save(_saveData);
		}
		_saveData.eventData.completedEvents = EventManager.GetCompletedEvents();
		_saveData.worldData.ActiveSceneIndex = SceneManager.GetActiveScene().buildIndex;
		_saveData.worldData.SpawnedTriggerData = SpawnerManager.GetTriggers();
		SaveSystem.Save(_saveData);
	}

	public static void LoadAll()
	{
		_saveData = SaveSystem.Load();
		new LoadSceneEvent("LoadCurrentScene", _saveData.worldData.ActiveSceneIndex);
		EventManager.Raise("LoadCurrentScene");

		for (int i = 0; i < GetCache().Count; i++)
		{
			GetCache()[i].Load(_saveData);
		}
		EventManager.LoadSavedEvents(_saveData.eventData.completedEvents);
		SpawnerManager.RestoreTriggersOnLoad(_saveData.worldData);
		
		if (_saveData.worldData.ActiveSceneIndex == 0) return;
		string firstEvent = (_saveData.worldData.ActiveSceneIndex) switch
		{
			1 => "ShiftLeader_Enter",
			2=> "KipEnterHouse_C1_S2",
			3=>"Chapter2_Intro",
			_ => ""
		};
		if(firstEvent != "" && !EventManager.GetCompletedEvents().Contains(firstEvent)) EventManager.Raise(firstEvent);
		else EventManager.Raise("Resume_Game");
	}
	private static List<ISaveable> GetCache()
	{
		return GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>().ToList();
	}

	private static void OnGameStateChange(ChangeGameStateEvent e)
	{
		CurrentGameState = e.State;
		EventManager.MarkEventCompleted(e.Id);
	}
}
