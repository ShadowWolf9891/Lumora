using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldSaveData
{
    public int ActiveSceneIndex;
	public List<NPCStatusData> NPCData = new();
}

[Serializable]
public class NPCStatusData
{
	public string InstanceId;
	public SerializableVector3 position;
	public PathStatus Status;
	public WalkType WalkType;
	public PathData PathData;
}

[Serializable]
public class PathData
{
	public int CurrentPath;
	public int CurrentPoint;
}
