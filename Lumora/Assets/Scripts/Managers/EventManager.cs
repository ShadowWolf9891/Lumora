using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Cinemachine;
using System.Linq;
using System.Collections;

[System.Serializable]
public class AllEvents
{
	public List<GameEventDefinition> allEvents;
}

public static class EventManager
{
	public static Queue<GameEventType> EventQueue { get; private set; }
	
	//Checked only when another event is completed. Used for if an event is fired but the requirements are not yet met.
	public static Queue<GameEventType> LazyEventQueue { get; private set; } 

	private static AllEvents allEventsDefs, c2EventsDefs, c1EventsDefs;
	private static Dictionary<string, GameEventType> _events;
	private static readonly Dictionary<Type, MethodInfo> _raiseCache = new();

	private static void LoadEvents()
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("events");
		TextAsset c2JsonFile = Resources.Load<TextAsset>("c2_events");
		TextAsset c1JsonFile = Resources.Load<TextAsset>("c1_events");
		allEventsDefs = JsonConvert.DeserializeObject<AllEvents>(jsonFile.text);
		c2EventsDefs = JsonConvert.DeserializeObject<AllEvents>(c2JsonFile.text);
		c1EventsDefs = JsonConvert.DeserializeObject<AllEvents> (c1JsonFile.text);
		_events = new Dictionary<string, GameEventType>();
		EventQueue = new Queue<GameEventType>();
		LazyEventQueue = new Queue<GameEventType>();
		
		foreach (GameEventDefinition eventDef in allEventsDefs.allEvents)
		{
			CreateEvent(eventDef);
			Debug.Log($"Created event {eventDef.id}");
		}
		foreach (GameEventDefinition eventDef in c2EventsDefs.allEvents)
		{
			CreateEvent(eventDef);
			Debug.Log($"Created event {eventDef.id}");
		}
        foreach (GameEventDefinition eventDef in c1EventsDefs.allEvents)
        {
            CreateEvent(eventDef);
            Debug.Log($"Created event {eventDef.id}");
        }
        Debug.Log("Loaded events json file.");
	}

	private static void CreateEvent(GameEventDefinition def)
	{
		GameEventType e = null;

		switch (def.type)
		{
			case "TestEvent":
				return;
			case "ChangeGameStateEvent":

				if (int.TryParse(def.parameters["state"], out int state))
				{
					e = new ChangeGameStateEvent(def.id, (GameStates)state);
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys} or state cannot be parsed.");
				}

				break;
			case "DialogueEvent":

				if (int.TryParse(def.parameters["chapter"], out int chapter) && int.TryParse(def.parameters["scene"], out int scene))
				{
					e = new DialogueEvent(def.id, chapter, scene);
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
			case "PathEvent":

				if (def.parameters.ContainsKey("npcName") && int.TryParse(def.parameters["newStatus"], out int newStatus))
				{
					if (newStatus < 0 || newStatus > (int)PathStatus.END_EARLY) //Change this if adding new statuses!
					{
						Debug.LogError($"Error parsing json events. newStatus is out of range of valid statuses.");
					}
					else
					{
						PathStatus ps = (PathStatus)newStatus;
						e = new PathEvent(def.id, def.parameters["npcName"], ps);
					}
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
			case "SpawnObjectEvent":
				if(def.parameters.ContainsKey("prefabName") && TryParseVector3(def.parameters["worldLocation"], out Vector3 spawnLocation))
				{
					Vector3 spawnRotation = Vector3.zero;
					if (def.parameters.ContainsKey("worldRotation"))
					{
						TryParseVector3(def.parameters["worldRotation"], out spawnRotation);
					}
					e = new SpawnObjectEvent(def.id, def.parameters["prefabName"], spawnLocation, spawnRotation);
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
			case "SpawnTriggerEvent":
				if (def.parameters.ContainsKey("eventToRaiseOnTrigger") && TryParseVector3(def.parameters["worldLocation"], out Vector3 triggerSpawnLocation))
				{
					float triggerRadius = 1f;
					LayerMask mask = ~0; //Everything
					if (def.parameters.ContainsKey("radius"))
					{
						 triggerRadius = float.TryParse(def.parameters["radius"], out float radius) ? radius : 1f;
					}
					
					if(def.parameters.ContainsKey("layerMask"))
					{
						string layerMaskName = def.parameters["layerMask"];
						mask = LayerMask.GetMask(layerMaskName);
					}
					e = new SpawnTriggerEvent(def.id, triggerSpawnLocation, def.parameters["eventToRaiseOnTrigger"], mask, triggerRadius);
					
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
			case "UIEvent":
				bool menuPopup = false;
				if (def.parameters.ContainsKey("menuPopup"))
				{
					menuPopup = bool.TryParse(def.parameters["menuPopup"], out bool popup) ? popup : false;
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				e = new SpawnPauseMenuEvent(def.id, menuPopup);
				break;
			case "UnlockAbilityEvent":
				if (def.parameters.ContainsKey("abilityName"))
				{
					e = new UnlockAbilityEvent(def.id, def.parameters["abilityName"]);
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
			case "BeginCutsceneEvent":
				if (def.parameters.ContainsKey("timelineName"))
				{
					float startTime = 0f;
					float endTime = -1f;
					if (def.parameters.ContainsKey("startTime"))
					{
						startTime = float.TryParse(def.parameters["startTime"], out float sTime) ? sTime : 0f;
					}
					if (def.parameters.ContainsKey("endTime"))
					{
						endTime = float.TryParse(def.parameters["endTime"], out float eTime) ? eTime : -1f;
					}

					e = new BeginCutsceneEvent(def.id, def.parameters["timelineName"], startTime, endTime);
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
			default:
				Debug.LogError($"Invalid type {def.type}");
				return;

		}

		if (e != null)
		{
			if (def.requireCompletedID != null && def.requireCompletedID != "")
			{
				e.RequireCompletedID = def.requireCompletedID;
			}
			e.IsCompleted = def.isCompleted;
			e.EventsToFire = def.eventsToFire;
			e.EventsOnComplete = def.eventsOnComplete;
			_events.Add(def.id, e);
		}
	}

	/// <summary>
	/// Raise an event from the events json file using it's string id. 
	/// </summary>
	/// <param name="eventID">The id of the event as it appears in the json file.</param>
	public static void Raise(string eventID)
	{
		if (_events == null) LoadEvents();

		if (!_events.TryGetValue(eventID, out var evt))
		{
			Debug.LogWarning($"Invalid event with id: {eventID}. Skipping...");
			return;
		}
		if(evt.IsCompleted)
		{
			Debug.LogWarning($"Event {eventID} has already been completed. Skipping...");
			return;
		}
		if(evt.RequireCompletedID != null && evt.RequireCompletedID != "")
		{
			if (_events.TryGetValue(evt.RequireCompletedID, out var completedEvent))
			{
				if(!completedEvent.IsCompleted)
				{
					LazyEventQueue.Enqueue(evt);
					return;
				}
			}
			else
			{
				Debug.LogWarning($"Invalid event with id: {evt.RequireCompletedID}. Skipping isCompleted check for event {eventID}.");
			}
		}

		EventQueue.Enqueue(evt);

		//Trigger events that are raised when this event is raised
		if (evt.EventsToFire != null && evt.EventsToFire.Length > 0) 
		{
			foreach(var e in evt.EventsToFire) 
			{
				Raise(e);
			}
		}
	}
	/// <summary>
	/// Handle the events in the queue. Called from GameManager.
	/// </summary>
	public static void HandleEvents()
	{
        if (EventQueue.Count > 0)
        {
			GameEventType evt = EventQueue.Dequeue();

			Type type = evt.GetType();
			//Use reflection to get the correct type if the value has not been cached already.
			if (!_raiseCache.TryGetValue(type, out var raiseMethod))
			{
				raiseMethod = typeof(GameEvents<>)
					.MakeGenericType(type)
					.GetMethod("Raise", BindingFlags.Public | BindingFlags.Static);
				_raiseCache[type] = raiseMethod;
			}

			raiseMethod?.Invoke(null, new object[] { evt });
			Debug.Log($"Raising event via {raiseMethod?.DeclaringType}::{raiseMethod?.Name}");
		}
    }

	/// <summary>
	/// Check if the lazy events have their requirements met after another event was completed and add them to the queue if they do.
	/// </summary>
	/// <param name="completedID">The ID of the event that was completed to trigger this method.</param>
	private static void CheckLazyEvents(string completedID)
	{
		if (LazyEventQueue.Count > 0)
		{
			List<GameEventType> lazyEventMatches = (List<GameEventType>)LazyEventQueue.Where(x => x.RequireCompletedID == completedID);

			foreach(var lazyEventMatch in lazyEventMatches)
			{
				EventQueue.Enqueue(lazyEventMatch);
			}
		}
	}

	/// <summary>
	/// Call this to mark an event as completed.
	/// </summary>
	/// <param name="eventID"></param>
	public static void MarkEventCompleted(string eventID)
	{
		if (_events == null) LoadEvents();

		if (!_events.TryGetValue(eventID, out var evt))
		{
			Debug.LogWarning($"Invalid event with id: {eventID}. Skipping...");
			return;
		}
		
		evt.IsCompleted = true;
		Debug.Log($"Completed event {eventID}");

		if (evt.EventsOnComplete != null && evt.EventsOnComplete.Length > 0)
		{
			foreach (var e in evt.EventsOnComplete)
			{
				Raise(e);

				Debug.Log($"Raised event {e}");
			}
		}

		CheckLazyEvents(eventID);
		
	}

	/// <summary>
	/// Helper to convert a sting in the format 0,0,0 to a vector 3.
	/// </summary>
	/// <param name="vector3AsString">The string to parse from the json file</param>
	/// <param name="parsedVec">The parsed vector3 passed back out if successful, 0,0,0 if not.</param>
	/// <returns>If the vector3 was successfully parsed</returns>
	private static bool TryParseVector3(string vector3AsString, out Vector3 parsedVec)
	{
		parsedVec = new Vector3();
		if (string.IsNullOrWhiteSpace(vector3AsString))
			return false;

		string[] substrings = vector3AsString.Split(",");

		if (substrings.Length != 3)
		{
			Debug.LogWarning($"Invalid Vector3 format: {vector3AsString}");
			return false;
		}

		//Try to parse the string as a float. Works for all number styles.
		if (float.TryParse(substrings[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
		float.TryParse(substrings[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
		float.TryParse(substrings[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
		{
			parsedVec = new Vector3(x, y, z);
			return true;
		}

		Debug.LogWarning($"Failed to parse Vector3: {vector3AsString}");
		return false;
	}


}
