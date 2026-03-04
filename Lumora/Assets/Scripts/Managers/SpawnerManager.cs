using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class for handling spawn events. Must be loaded in Awake() with SpawnerManager.Load()
/// </summary>
public class SpawnerManager : MonoBehaviour
{
	public static SpawnerManager Instance;

	[SerializeField] SpawnableObjects allSpawnables;
	private List<SpawnedTriggerData> triggers;
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
		GameEvents<SpawnObjectEvent>.Subscribe(SpawnObject);
		GameEvents<SpawnTriggerEvent>.Subscribe(SpawnTrigger);
	}
	private void OnDisable()
	{
		GameEvents<SpawnObjectEvent>.Unsubscribe(SpawnObject);
		GameEvents<SpawnTriggerEvent>.Unsubscribe(SpawnTrigger);
	}
	private void SpawnObject(SpawnObjectEvent e)
	{
		GameObject objectToSpawn = allSpawnables.objectList.Find(obj => obj.name == e.PrefabName);

		if (objectToSpawn != null)
		{
			GameObject.Instantiate(objectToSpawn, e.Position, Quaternion.Euler(e.Rotation));
		}
	}
	private void SpawnTrigger(SpawnTriggerEvent e)
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
		EventManager.Instance.MarkEventCompleted(e.Id);
	}
	public List<SpawnedTriggerData> GetTriggers() => triggers;
	public void Reset() => triggers?.Clear();
	public void RestoreTriggersOnLoad(WorldSaveData saveData)
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
	public void MarkTriggered(string triggerID, GameObject triggerGO)
	{
		var trigger = triggers.Find(x => x.EventId == triggerID);
		trigger.Triggered = true;
		triggers.Remove(trigger);
		Destroy(triggerGO);
	}
}
