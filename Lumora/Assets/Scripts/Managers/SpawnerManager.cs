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

	public static void Load(SpawnableObjects objectList)
	{
		if(objectList == null) { Debug.LogError("List of spawnable objects has not been loaded."); return; }
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
		}


	}
}
