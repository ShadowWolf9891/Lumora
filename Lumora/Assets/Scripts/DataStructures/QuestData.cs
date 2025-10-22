using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
	public string id;
	public string description;
	public QuestStatus status = QuestStatus.INCOMPLETE;
	public QuestData parentQuest = null;
	public QuestData[] subQuests = null;
}
[System.Serializable]
public class AllQuests { public QuestData[] questData; }
public enum QuestStatus
{
	INCOMPLETE = 0,
	INPROGRESS = 1,
	COMPLETED = 2,
	FAILED = 3
}


