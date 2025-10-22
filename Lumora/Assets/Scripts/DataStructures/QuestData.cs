using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
	public string id;
	public string description;
	public int status = 0;
	public QuestData[] subQuests = null;
}
public enum QuestStatus
{
	INCOMPLETE = 0,
	INPROGRESS = 1,
	COMPLETED = 2,
	FAILED = 3
}


