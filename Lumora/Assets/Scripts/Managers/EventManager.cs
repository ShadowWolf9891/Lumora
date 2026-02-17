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

	private static AllEvents allEventsDefs, c2EventsDefs, c1EventsDefs, c1s2EventsDefs;
	private static Dictionary<string, GameEventType> _events;
	private static readonly Dictionary<Type, MethodInfo> _raiseCache = new();

	//Check in completed events when saving / loading
	private static HashSet<string> _completedEvents = new();

	private static void LoadEvents()
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("events");
		TextAsset c2JsonFile = Resources.Load<TextAsset>("c2_events");
		TextAsset c1JsonFile = Resources.Load<TextAsset>("c1_events");
        TextAsset c1s2JsonFile = Resources.Load<TextAsset>("c1_s2_events");
        allEventsDefs = JsonConvert.DeserializeObject<AllEvents>(jsonFile.text);
		c2EventsDefs = JsonConvert.DeserializeObject<AllEvents>(c2JsonFile.text);
		c1EventsDefs = JsonConvert.DeserializeObject<AllEvents> (c1JsonFile.text);
		c1s2EventsDefs = JsonConvert.DeserializeObject<AllEvents>(c1s2JsonFile.text);
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
        foreach (GameEventDefinition eventDef in c1s2EventsDefs.allEvents)
        {
            CreateEvent(eventDef);
            Debug.Log($"Created event {eventDef.id}");
        }

        Debug.Log("Loaded events json file.");
	}
	public static void LoadSavedEvents(List<string> completedEvents)
	{
		if (_events == null) LoadEvents();
	
		foreach (string eId in completedEvents) { _completedEvents.Add(eId); }
	}
	public static List<string> GetCompletedEvents()
	{
		return _completedEvents.ToList();
	}
	private static void CreateEvent(GameEventDefinition def)
	{
		GameEventType e = def.type switch
		{
			"TestEvent" => null,
			"DummyEvent" => new DummyEvent(def.id),
			"ChangeGameStateEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("state"), out int state) ?
				new ChangeGameStateEvent(def.id, (GameStates)state) : null,
			"DialogueEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("chapter"), out int chapter) &&
				IsValidParam(def.parameters.GetValueOrDefault("scene"), out int scene) ?
				new DialogueEvent(def.id, chapter, scene) : null,
			"PathEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("npcName"), out string npcName) &&
				IsValidParam(def.parameters.GetValueOrDefault("newStatus"), out PathStatus newStatus) ?
				new PathEvent(def.id, npcName, newStatus) : null,
			"ChangeNPCWalkTypeEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("npcName"), out string npcName) &&
				IsValidParam(def.parameters.GetValueOrDefault("mode"), out WalkType mode) &&
				IsValidParam(def.parameters.GetValueOrDefault("target"), out string target, true) &&
				IsValidParam(def.parameters.GetValueOrDefault("followDistance"), out int distance, true) ?
				new ChangeNPCWalkTypeEvent(def.id, npcName, mode,
					target != default ? target : mode == WalkType.NORMAL ? null : "Player",
					distance != default ? distance : 5f) : null,
			"SpawnObjectEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("prefabName"), out string prefabName) &&
				IsValidParam(def.parameters.GetValueOrDefault("worldLocation"), out Vector3 worldLocation) &&
				IsValidParam(def.parameters.GetValueOrDefault("worldRotation"), out Vector3 worldRotation, true) ?
				new SpawnObjectEvent(def.id, prefabName, worldLocation, worldRotation != default ? worldRotation : Vector3.zero) : null,
			"SpawnTriggerEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("worldLocation"), out Vector3 worldLocation) &&
				IsValidParam(def.parameters.GetValueOrDefault("radius"), out float radius, true) &&
				IsValidParam(def.parameters.GetValueOrDefault("layerMask"), out LayerMask mask, true) &&
				IsValidParam(def.parameters.GetValueOrDefault("eventToRaiseOnTrigger"), out string eventName) ?
				new SpawnTriggerEvent(def.id, worldLocation, eventName, mask != default ? mask : ~0, radius != default ? radius : 1f) : null,
			"UIEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("menuPopup"), out bool menuPopup) ?
				new SpawnPauseMenuEvent(def.id, menuPopup != default && menuPopup) : null,
			"UnlockAbilityEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("abilityName"), out string abilityName) ?
				new UnlockAbilityEvent(def.id, abilityName) : null,
			"LoadSceneEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("sceneIndex"), out int sceneID) ?
				new LoadSceneEvent(def.id, sceneID) : null,
			"BeginCutsceneEvent" =>
				IsValidParam(def.parameters.GetValueOrDefault("timelineName"), out string timelineName) &&
				IsValidParam(def.parameters.GetValueOrDefault("startTime"), out float startTime, true) &&
				IsValidParam(def.parameters.GetValueOrDefault("endTime"), out float endTime, true) ?
				new BeginCutsceneEvent(def.id, timelineName, startTime != default ? startTime : 0f, endTime != default ? endTime : 0f) : null,
			"ToggleVisibilityEvent" => 
				IsValidParam(def.parameters.GetValueOrDefault("objectName"), out string objectName) &&
				IsValidParam(def.parameters.GetValueOrDefault("isVisible"), out bool isVisible) ?
				new ToggleVisibilityEvent(def.id, objectName, isVisible) : null,
			"" =>null,
			_ => throw new NotImplementedException()
		};

		if (e != null)
		{
			if (def.requireCompletedID != null && def.requireCompletedID != "")
			{
				e.RequireCompletedID = def.requireCompletedID;
			}
			e.IsRepeatable = def.isRepeatable == "true";
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
		if (_completedEvents.Contains(evt.Id))
		{
			Debug.LogWarning($"Event {eventID} has already been completed. Skipping...");
			return;
		}
		if (evt.RequireCompletedID != null && evt.RequireCompletedID != "")
		{
			if (_events.TryGetValue(evt.RequireCompletedID, out var completedEvent))
			{
				if (!_completedEvents.Contains(completedEvent.Id))
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

		//Queue this event if it is not a dummy event
		if (evt is not DummyEvent)
		{
			EventQueue.Enqueue(evt);
		}

		//Trigger events that are raised when this event is raised
		if (evt.EventsToFire != null && evt.EventsToFire.Length > 0)
		{
			foreach (var e in evt.EventsToFire)
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
		if (_events == null) LoadEvents();
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
			Debug.Log($"Handling event {evt.Id} of type {raiseMethod?.DeclaringType}");
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
	public static void MarkEventCompleted(string eventID, bool skipOnComplete = false)
	{
		if (_events == null) LoadEvents();

		if (!_events.TryGetValue(eventID, out var evt))
		{
			Debug.LogWarning($"Invalid event with id: {eventID}. Skipping...");
			return;
		}
		if(evt is DummyEvent)
		{
			//Exit if not all events to fire have been marked complete for dummy
			if(!CheckDummyComplete(evt as DummyEvent)) return; 
		}
		if(!evt.IsRepeatable) _completedEvents.Add(eventID);
		Debug.Log($"Completed event {eventID}");

		if(!skipOnComplete) RaiseEventsOnComplete(eventID);

		CheckLazyEvents(eventID);
		GameManager.SaveAll();
	}

	private static void RaiseEventsOnComplete(string eventID)
	{
		if(_completedEvents.Contains(eventID))
		{
			if (!_events.TryGetValue(eventID, out var evt)) return;
			if (evt.EventsOnComplete == null || evt.EventsOnComplete.Count() <= 0) return;

			foreach (var e in evt.EventsOnComplete)
			{ 
				if(_completedEvents.Contains(e)) continue;

				Raise(e);
			}
		}
	}

	private static bool CheckDummyComplete(DummyEvent e)
	{
		foreach (var checkEvent in e.EventsToFire)
		{
			if (!_events.TryGetValue(checkEvent, out var evt))
			{
				Debug.LogWarning($"Invalid event with id: {checkEvent}. Skipping...");
				return false;
			}
			if(!_completedEvents.Contains(evt.Id))return false;
		}
		return true;
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
		return false;
	}
	/// <summary>
	/// Dictionary that checks to see if a string is valid as a specific type and returns the parsed value
	/// </summary>
	private static Dictionary<Type, Func<string, (bool success, object value)>> parsers = new Dictionary<Type, Func<string, (bool success, object value)>>()
	{
		[typeof(int)] = s => (int.TryParse(s, out int v), v),
		[typeof(float)] = s => (float.TryParse(s, out float v), v),
		[typeof(bool)] = s => (bool.TryParse(s, out bool v), v),
		[typeof(Vector3)] = s =>(TryParseVector3(s, out Vector3 v),v),
		[typeof(string)] = s => (true, s),
		[typeof(PathStatus)] = s =>(int.TryParse(s, out int v), (PathStatus)v),
		[typeof(WalkType)] = s => (int.TryParse(s, out int v), (WalkType)v),
		[typeof(LayerMask)] = s => (LayerMask.GetMask(s)!=default, LayerMask.GetMask(s))
		
	};
	/// <summary>
	/// Check if a parameter value is valid for the type it is trying to be parsed to. Passes the parsed value back if successful.
	/// </summary>
	/// <typeparam name="T">The type to try and parse to</typeparam>
	/// <param name="paramValue">The string of the parameter's value from the json file</param>
	/// <param name="parsedValue">The parsed value that is passed back out if successful. If unsuccessful, this is null</param>
	/// <returns></returns>
	private static bool IsValidParam<T>(string paramValue, out T parsedValue, bool isOptional = false)
	{
		parsedValue = default;
		if (paramValue == null || paramValue == "") return isOptional; //Optional values are valid if they don't exist and will use a default value.
		if (!parsers.TryGetValue(typeof(T), out var parser)) return false;
		var (success, value) = parser(paramValue);
		if (!success)
		{
			Debug.LogError($"Json parse error! Value: {paramValue} cannot be converted to {typeof(T)}.");
			return false;
		}
		if (typeof(T).IsEnum)parsedValue = (T)Enum.ToObject(typeof(T), value);
		else if (typeof(T) == typeof(LayerMask))parsedValue = (T)(object)(LayerMask)(int)value;
		else parsedValue = (T)Convert.ChangeType(value, typeof(T)); //Might break later :)
		return true;
	}
}
