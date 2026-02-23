using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Static class for handling spawn events. Must be loaded in Awake() with SpawnerManager.Load()
/// </summary>
public static class SpawnerManager
{
	private static bool _isLoaded = false;

	private static SpawnableObjects allSpawnables;

	private static List<SpawnedTriggerData> triggers;
	public static void Load(SpawnableObjects objectList)
	{
		if (_isLoaded) return;
		if (objectList == null) { Debug.LogError("List of spawnable objects has not been loaded."); return; }
		allSpawnables = objectList;
		GameEvents<SpawnObjectEvent>.Subscribe(SpawnObject);
		GameEvents<SpawnTriggerEvent>.Subscribe(SpawnTrigger);
		Debug.Log("Loaded Spawner");
		_isLoaded = true;
	}
	private static void SpawnObject(SpawnObjectEvent e)
	{
		GameObject objectToSpawn = allSpawnables.objectList.Find(obj => obj.name == e.PrefabName);

		if (objectToSpawn != null)
		{
			GameObject.Instantiate(objectToSpawn, e.Position, Quaternion.Euler(e.Rotation));
		}

	}
	private static void SpawnTrigger(SpawnTriggerEvent e)
	{
		GameObject triggerSpawn = allSpawnables.objectList.Find(obj => obj.name == "Spawnable_Trigger");

		if (triggerSpawn != null)
		{
			GameObject temp = GameObject.Instantiate(triggerSpawn, e.Position, Quaternion.identity);
			temp.GetComponent<SpawnableTriggerBehavior>().Initialize(e.Id, e.EventToRaiseOnTrigger, e.layerMask, e.Radius, e.IsRepeatable);
			triggers ??= new();
			// Store in world state so it survives save/load
			triggers.Add(new SpawnedTriggerData
			{
				EventId = e.Id,
				EventToRaiseOnTrigger = e.EventToRaiseOnTrigger,
				Position = new SerializableVector3(e.Position),
				LayerMask = (int)e.layerMask,
				Radius = e.Radius,
				IsRepeatable = e.IsRepeatable,
				Triggered = false
			}); 
		}
		EventManager.MarkEventCompleted(e.Id);
	}
	public static List<SpawnedTriggerData> GetTriggers()
	{
		return triggers;
	}
	public static void Reset() => triggers?.Clear();
	public static void RestoreTriggersOnLoad(WorldSaveData saveData)
	{
		if (saveData == null || saveData.SpawnedTriggerData == null || saveData.SpawnedTriggerData.Count <=0) return;
		triggers = saveData.SpawnedTriggerData;
		foreach (var triggerData in saveData.SpawnedTriggerData)
		{
			if (!triggerData.Triggered)
			{
				GameObject triggerSpawn = allSpawnables.objectList.Find(obj => obj.name == "Spawnable_Trigger");
				if (triggerSpawn != null)
				{
					GameObject temp = GameObject.Instantiate(triggerSpawn, triggerData.Position.ToVector3(), Quaternion.identity);
					temp.GetComponent<SpawnableTriggerBehavior>()
						.Initialize(triggerData.EventId, triggerData.EventToRaiseOnTrigger,
									(LayerMask)triggerData.LayerMask, triggerData.Radius, triggerData.IsRepeatable);
				}
			}
		}
	}
	public static void MarkTriggered(string triggerID) => triggers.Find(x => x.EventId == triggerID).Triggered = true;
}
