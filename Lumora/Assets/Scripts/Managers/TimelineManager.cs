using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public static class TimelineManager
{
	public static List<PlayableDirector> _directors { get; private set; }

	public static void Load()
	{
		//Might only load for first scene
		_directors = GameObject.FindObjectsByType<PlayableDirector>(FindObjectsSortMode.InstanceID).ToList();
		GameEvents<BeginCutsceneEvent>.Subscribe(StartTimeline);
	}

	/// <summary>
	/// Start a cutscene at a specific start time using the begin cutscene event. 
	/// Use Raise("EventName") or load it from the json file.
	/// </summary>
	private static void StartTimeline(BeginCutsceneEvent e)
	{
		PlayableDirector director = _directors.Find(director => director.gameObject.name == e.TimelineName);
		if (director == null)
		{
			Debug.LogError($"Unable to find gameobject with name {e.TimelineName} in the scene. " +
				$"Make sure you are calling Load() when the scene loads.");
			return;
		}

		if(director.state != PlayState.Playing) 
		{
			director.time = e.StartTime;
			director.Play();
		}

		//TODO: Implement End time to end the timeline when it reaches that point.

	}
}
