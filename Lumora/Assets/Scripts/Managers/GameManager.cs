using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	[SerializeField] LayerMask coverLayerMask;

	List<GameObject> _cachedObjects = new();

	public GameStates CurrentGameState { get; private set; }
	public GameStates PreviousGameState { get; private set; }

	private GameSaveData _saveData;
	private bool _loaded = false;

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
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	private void OnDisable()
	{
		GameEvents<ToggleVisibilityEvent>.Unsubscribe(ToggleVisibility);
		GameEvents<ChangeGameStateEvent>.Unsubscribe(OnGameStateChange);
		GameEvents<DeleteSaveEvent>.Unsubscribe(DeleteSave);
		GameEvents<EnableSaveEvent>.Unsubscribe(EnableSaving);
		GameEvents<LoadSceneEvent>.Unsubscribe(LoadScene);
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}
	private async void LoadScene(LoadSceneEvent e)
	{
		await SceneManager.LoadSceneAsync(e.SceneIndex);
	}
	private void OnSceneUnloaded(Scene scene)
	{
		_loaded = false;
	}

	private void Update()
	{
		if (!_loaded) {
			if (!SaveSystem.HasSaved && GameConfig.Mode == GameMode.Production) SaveAll();
			LoadAll(SceneManager.GetActiveScene().buildIndex);
			_loaded = true;
		}
		if(_loaded) EventManager.Instance.HandleEvents(); //Handle events each frame if there are any.
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
		CameraManager.Instance.Reset();
		TimelineManager.Instance.Load();

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

		CameraManager.Instance.LoadCameras();
		if (SceneManager.GetSceneByBuildIndex(curScene).name == "MainMenu") return;

		string firstEvent = (SceneManager.GetSceneByBuildIndex(curScene).name) switch
		{
			"Chapter1-Mine" => "ShiftLeader_Enter",
			"Chapter1-House" => "KipEnterHouse_C1_S2",
			"Chapter2_Stealth"=>"Chapter2_Intro",
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
		PreviousGameState = CurrentGameState;
		CurrentGameState = e.State;
		EventManager.Instance.MarkEventCompleted(e.Id);
	}
}
