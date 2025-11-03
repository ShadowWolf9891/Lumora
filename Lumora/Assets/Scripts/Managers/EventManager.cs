using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class AllEvents
{
	public List<GameEventDefinition> allEvents;
}

public static class EventManager
{
	private static AllEvents allEventsDefs, c2EventsDefs;
	private static Dictionary<string, GameEventType> _events;
	private static readonly Dictionary<Type, MethodInfo> _raiseCache = new();

	private static void LoadEvents()
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("events");
		TextAsset c2JsonFile = Resources.Load<TextAsset>("c2_events");
		allEventsDefs = JsonConvert.DeserializeObject<AllEvents>(jsonFile.text);
		c2EventsDefs = JsonConvert.DeserializeObject<AllEvents>(c2JsonFile.text);
		_events = new Dictionary<string, GameEventType>();
		
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
			case "NPCMovementEvent":

				if (def.parameters.ContainsKey("npcToMove") && TryParseVector3(def.parameters["targetLocation"], out Vector3 location))
				{
					e = new NPCMovementEvent(def.id, def.parameters["npcToMove"], location);
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
			case "SpawnObjectEvent":
				if(def.parameters.ContainsKey("prefabName") && TryParseVector3(def.parameters["worldLocation"], out Vector3 spawnLocation))
				{ 
					if(!TryParseVector3(def.parameters["worldRotation"], out Vector3 spawnRotation)){spawnRotation = Vector3.zero;}
					e = new SpawnObjectEvent(def.id, def.parameters["prefabName"], spawnLocation, spawnRotation);
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
						mask = LayerMask.GetMask(def.parameters["layerMask"]);
					}
					e = new SpawnTriggerEvent(def.id, triggerSpawnLocation, def.parameters["evnetToRaiseOnTrigger"], mask, triggerRadius);
					
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
		if( evt.IsCompleted) 
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
					Debug.Log($"Event {completedEvent.Id} is not completed so {eventID} will not fire. Skipping...");
					return;
				}
			}
			else
			{
				Debug.LogWarning($"Invalid event with id: {evt.RequireCompletedID}. Skipping isCompleted check for event {eventID}.");
			}
		}
		
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
