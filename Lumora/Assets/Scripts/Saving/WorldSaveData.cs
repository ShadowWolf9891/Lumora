using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldSaveData
{
    public int ActiveSceneIndex;
	public List<NPCStatusData> NPCData = new();
	public List<SpawnedTriggerData> SpawnedTriggerData = new();
}

[Serializable]
public class NPCStatusData
{
	public string InstanceId;
	public SerializableVector3 position;
	public PathStatus Status;
	public WalkType WalkType;
	public PathData PathData;
	public string ActiveEventID;
}

[Serializable]
public class PathData
{
	public int CurrentPath;
	public int CurrentPoint;
}

[Serializable]
public class SpawnedTriggerData
{
	public string EventId;            // SpawnTriggerEvent ID
	public string EventToRaiseOnTrigger; // Event the trigger fires on enter
	public SerializableVector3 Position;
	public int LayerMask;
	public float Radius;
	public bool Triggered;            // Has the player already triggered this?
	public bool IsRepeatable;
}