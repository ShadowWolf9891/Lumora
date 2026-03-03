using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	[SerializeField] LayerMask coverLayerMask;

	List<GameObject> _cachedObjects = new();

	public GameStates CurrentGameState { get; private set; }

	private GameSaveData _saveData;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);
	}

	private void OnEnable()
	{
		GameEvents<ToggleVisibilityEvent>.Subscribe(ToggleVisibility);
		GameEvents<ChangeGameStateEvent>.Subscribe(OnGameStateChange);
		GameEvents<DeleteSaveEvent>.Subscribe(DeleteSave);
		GameEvents<EnableSaveEvent>.Subscribe(EnableSaving);
		GameEvents<LoadSceneEvent>.Subscribe(LoadScene);
	}
	private void OnDisable()
	{
		GameEvents<ToggleVisibilityEvent>.Unsubscribe(ToggleVisibility);
		GameEvents<ChangeGameStateEvent>.Unsubscribe(OnGameStateChange);
		GameEvents<DeleteSaveEvent>.Unsubscribe(DeleteSave);
		GameEvents<EnableSaveEvent>.Unsubscribe(EnableSaving);
		GameEvents<LoadSceneEvent>.Unsubscribe(LoadScene);
	}
	private async void LoadScene(LoadSceneEvent e)
	{
		await SceneManager.LoadSceneAsync(e.SceneIndex);
	}
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!SaveSystem.HasSaved && GameConfig.Mode == GameMode.Production) SaveAll();
		TimelineManager.Instance.Load();
		CameraManager.Instance.Reset();
		LoadAll(scene.buildIndex);
		CameraManager.Instance.LoadCameras();
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;

		EventManager.Instance.Raise(new LoadSceneEvent("LoadCurrentScene", 1));
		//EventManager.Raise("UnlockThrow");
		//EventDispatcher.DispatchForCurrentQuest("SubQuest1");
	}


	private void Update()
	{
		EventManager.Instance.HandleEvents(); //Handle events each frame if there are any.
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
	private void EnableSaving(EnableSaveEvent e)
	{
		GameConfig.Mode = GameMode.Production;
	}
	public void SaveAll()
	{
		if (GameConfig.Mode == GameMode.Playtest) return;
		_saveData ??= new GameSaveData();
		_saveData.eventData ??= new EventSaveData();
		_saveData.playerData ??= new PlayerSaveData();
		_saveData.worldData ??= new WorldSaveData();
		for(int i = 0; i < GetCache().Count; i++)
		{
			GetCache()[i].Save(_saveData);
		}
		_saveData.eventData.completedEvents = EventManager.Instance.GetCompletedEvents();
		_saveData.worldData.ActiveSceneIndex = SceneManager.GetActiveScene().buildIndex;
		_saveData.worldData.SpawnedTriggerData = SpawnerManager.Instance.GetTriggers();
		SaveSystem.Save(_saveData);
	}

	public void LoadAll(int curScene)
	{
		if (GameConfig.Mode == GameMode.Production)
		{
			_saveData = SaveSystem.Load(curScene);

			for (int i = 0; i < GetCache().Count; i++)
			{
				GetCache()[i].Load(_saveData);
			}

			EventManager.Instance.LoadSavedEvents(_saveData.eventData.completedEvents);
			SpawnerManager.Instance.RestoreTriggersOnLoad(_saveData.worldData);
		}
		if (curScene == 0) return;
		string firstEvent = (curScene) switch
		{
			1 => "ShiftLeader_Enter",
			2=> "KipEnterHouse_C1_S2",
			3=>"Chapter2_Intro",
			_ => ""
		};
		if (firstEvent != "" && !EventManager.Instance.GetCompletedEvents().Contains(firstEvent))
		{
			EventManager.Instance.Raise(firstEvent);
			Debug.Log($"Raised first event {firstEvent}");
		}
		else EventManager.Instance.Raise("Resume_Game");
	}

	public void DeleteSave(DeleteSaveEvent e)
	{
		EventManager.Instance.Reset(); //Might break for completed events between scenes.
		SpawnerManager.Instance.Reset();
		CameraManager.Instance.Reset();
		_saveData = null;
		SaveSystem.DeleteData();
	}
	private List<ISaveable> GetCache()
	{
		return GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>().ToList();
	}

	private void OnGameStateChange(ChangeGameStateEvent e)
	{
		CurrentGameState = e.State;
		EventManager.Instance.MarkEventCompleted(e.Id);
	}
}
