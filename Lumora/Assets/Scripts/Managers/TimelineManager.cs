using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{
	public static TimelineManager Instance;
	public List<PlayableDirector> _directors { get; private set; }

	public Dictionary<string, PlayableDirector> _eventTracker = new();
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
		foreach (var d in _directors)
			d.stopped += OnDirectorStopped;
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
		if (director.state != PlayState.Playing) 
		{
			director.time = e.StartTime;
			director.Play();
		}

		//TODO: Implement End time to end the timeline when it reaches that point.
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
		}
	}

}
