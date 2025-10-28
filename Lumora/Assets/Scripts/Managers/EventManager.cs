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
	private static AllEvents allEventsDefs;
	private static Dictionary<string, GameEventType> _events;
	private static readonly Dictionary<Type, MethodInfo> _raiseCache = new();

	private static void LoadEvents()
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("events");
		allEventsDefs = JsonConvert.DeserializeObject<AllEvents>(jsonFile.text);
		_events = new Dictionary<string, GameEventType>();

		foreach (GameEventDefinition eventDef in allEventsDefs.allEvents)
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

				if (def.parameters.ContainsKey("npcToMove") && TryParseVector3(def.parameters["targetLocation"], out Vector3 location) && TryParseVector3(def.parameters["targetRotation"], out Vector3 rotation))
				{
					e = new NPCMovementEvent(def.id, def.parameters["npcToMove"], location, rotation);
				}
				else
				{
					Debug.LogError($"Error parsing json events. {def.type} does not contain a definition for {def.parameters.Keys}");
				}
				break;
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
