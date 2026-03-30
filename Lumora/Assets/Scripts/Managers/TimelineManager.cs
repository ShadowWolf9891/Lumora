using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class TimelineManager : MonoBehaviour
{
	public static TimelineManager Instance;
	public List<PlayableDirector> _directors { get; private set; }
	public Dictionary<string, PlayableDirector> _eventTracker = new();
	private Dictionary<string, DirectorEntry> _directorEntries = new();
	private CinemachineBrain _brain;


	// Store both the director and its optional end time
	private struct DirectorEntry
	{
		public PlayableDirector Director;
		public double? EndTime; // null means play to completion
	}

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
		GameEvents<BeginCutsceneEvent>.Subscribe(StartTimeline);
	}
	private void OnDisable()
	{
		GameEvents<BeginCutsceneEvent>.Unsubscribe(StartTimeline);
	}
	public void Load()
	{
		//Might only load for first scene
		_directors = GameObject.FindObjectsByType<PlayableDirector>(FindObjectsSortMode.InstanceID).ToList();
		_brain = GameObject.FindFirstObjectByType<CinemachineBrain>();
		foreach (var d in _directors)
			d.stopped += OnDirectorStopped;
	}

	private void Update()
	{
		// Check each active director to see if it has passed its end time
		foreach (var kvp in _directorEntries)
		{
			var entry = kvp.Value;
			if (entry.EndTime.HasValue &&
				entry.Director.state == PlayState.Playing &&
				entry.Director.time >= entry.EndTime.Value)
			{
				entry.Director.time = entry.EndTime.Value;
				entry.Director.Stop(); // triggers OnDirectorStopped
			}
		}
	}
	/// <summary>
	/// Start a cutscene at a specific start time using the begin cutscene event. 
	/// Use Raise("EventName") or load it from the json file.
	/// </summary>
	private void StartTimeline(BeginCutsceneEvent e)
	{
		PlayableDirector director = _directors.Find(director => director.gameObject.name == e.TimelineName);
        if (director == null)
		{
			Debug.LogError($"Unable to find gameobject with name {e.TimelineName} in the scene. " +
				$"Make sure you are calling Load() when the scene loads.");
			return;
		}
		if (!_eventTracker.ContainsKey(e.Id))
		{
			_eventTracker.Add(e.Id, director);
		}
		double? endTime = (e.EndTime > 0 && e.EndTime > e.StartTime) ? e.EndTime : null;

		if (!_directorEntries.ContainsKey(e.Id))
		{
			_directorEntries.Add(e.Id, new DirectorEntry
			{
				Director = director,
				EndTime = endTime
			});
		}

		if (director.state != PlayState.Playing)
		{
			var timeline = director.playableAsset as TimelineAsset;
			foreach (var track in timeline.GetOutputTracks())
			{
				if (track is CinemachineTrack)
				{
					director.SetGenericBinding(track, _brain);
					break;
				}
			}

			director.time = e.StartTime;
			director.Play();
			EventManager.Instance.Raise(new ChangeGameStateEvent("Start_Cutscene", GameStates.Cutscene));
		}
	}
	private void OnDirectorStopped(PlayableDirector director)
	{
		var completedKeys = new List<string>();

		foreach (var ev in _eventTracker)
		{
			if (ev.Value == director)
				completedKeys.Add(ev.Key);
		}

		foreach (var key in completedKeys)
		{
			EventManager.Instance.MarkEventCompleted(key);
			_eventTracker.Remove(key);
			_directorEntries.Remove(key);
		}

		//Must resume game from json file on cutscene end!!


		//EventManager.Instance.Raise(new ChangeGameStateEvent("End_Cutscene", GameManager.Instance.PreviousGameState));
	}

}
