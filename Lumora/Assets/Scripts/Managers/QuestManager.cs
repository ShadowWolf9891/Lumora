using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class QuestManager
{
    private static QuestData data = null;
	private static List<QuestData> inProgressQuests;

	/// <summary>
	/// Load the quests from the json file
	/// </summary>
    private static void LoadQuests()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("quests");
        data = JsonUtility.FromJson<QuestData>(jsonFile.text);
		inProgressQuests = new List<QuestData>();
		Debug.Log($"Loaded quests json file. Root ID: {data.id}");
	}

	public static void DebugPrintData(QuestData curData)
	{
		Debug.Log($"{curData.id} is {(QuestStatus)curData.status} ");
		if (curData.subQuests != null && curData.subQuests.Length > 0)
		{
			foreach (var quest in curData.subQuests)
			{
				DebugPrintData(quest);
			}
		}
	}
	public static List<QuestData> GetInProgress()
	{
		return inProgressQuests;
	}

	/// <summary>
	/// Start a quest or quest chain 
	/// </summary>
	/// <param name="questChainID">The quest id to start. If it has a child, that one becomes in progress.</param>
	public static void StartQuest(string questChainID)
    {
		if (data == null) { LoadQuests(); }
		
		QuestData qData = GetQuest(data, questChainID);
		if(qData == null)
		{
			Debug.LogError($"There is no quest with the id {questChainID}");
			return;
		}
		StartQuestChain(qData);
	}
	/// <summary>
	/// Progress the current quest in the chain to the next one, or complete the chain.
	/// </summary>
	/// <param name="questChainID">The quest id to progress the children of.</param>
	public static void ProgressQuest(string questChainID) 
	{
		QuestData qData = GetQuest(data, questChainID);
		ProgressQuest(qData);
		//DebugPrintData(data);
	}
	private static QuestData ProgressQuest(QuestData curQuest)
	{
		// Sanity check
		if (curQuest == null)
			return null;

		// If current quest has subquests, go deeper first
		if (curQuest.subQuests != null && curQuest.subQuests.Length > 0)
		{
			foreach (var sub in curQuest.subQuests)
			{
				if (sub.status == (int)QuestStatus.INPROGRESS)
				{
					// Recurse into subquest
					var completedQuest = ProgressQuest(sub);

					// If something was completed deeper, handle next sibling logic here
					if (completedQuest != null)
					{
						// Find next sibling
						var siblings = curQuest.subQuests;
						int index = Array.IndexOf(siblings, completedQuest);

						if (index + 1 < siblings.Length)
						{
							var next = siblings[index + 1];
							// Mark next sibling and its entire first child path as INPROGRESS
							var leaf = MarkFirstChildBranchInProgress(next);
							return leaf;
						}
						else
						{
							// No more siblings complete parent
							curQuest.status = (int)QuestStatus.COMPLETED;
							if (!inProgressQuests.Contains(curQuest))
							{
								inProgressQuests.Add(curQuest);
							}
							return curQuest;
						}
					}
				}
			}
		}

		//If this is a leaf and in progress, mark as completed
		if (curQuest.status == (int)QuestStatus.INPROGRESS)
		{
			curQuest.status = (int)QuestStatus.COMPLETED;
			if (inProgressQuests.Contains(curQuest))
			{
				inProgressQuests.Remove(curQuest);
			}
			return curQuest;
		}

		return null; // Nothing to do
	}
	/// <summary>
	/// Convert string id to QuestData
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	private static QuestData GetQuest(QuestData parent, string id)
	{
		if (parent == null)
			return null;

		if (parent.id == id)
			return parent;

		if (parent.subQuests == null)
			return null;

		foreach (var sub in parent.subQuests)
		{
			var result = GetQuest(sub, id);
			if (result != null)
				return result;
		}

		return null;
	}
	private static void StartQuestChain(QuestData parentQuest)
	{
		if (parentQuest == null)
		{
			Debug.LogWarning("Tried to start quest chain but parent quest is null.");
			return;
		}

		// If it’s already started, no need to restart
		if (parentQuest.status == (int)QuestStatus.INPROGRESS)
			return;

		// Mark the parent as active
		parentQuest.status = (int)QuestStatus.INPROGRESS;
		if (!inProgressQuests.Contains(parentQuest))
		{
			inProgressQuests.Add(parentQuest);
		}

		// If there are no subquests, just start the parent
		if (parentQuest.subQuests == null || parentQuest.subQuests.Length == 0)
			return;

		// Unlock and start the first subquest only
		StartQuestChain(parentQuest.subQuests[0]);

		Debug.Log($"Started quest chain: {parentQuest.id}. First quest: {parentQuest.subQuests[0].id}");
	}
	private static QuestData MarkFirstChildBranchInProgress(QuestData quest)
	{
		if (quest == null)
			return null;

		// Mark this node as INPROGRESS if not completed
		if (quest.status != (int)QuestStatus.COMPLETED)
		{
			quest.status = (int)QuestStatus.INPROGRESS;
			if (!inProgressQuests.Contains(quest))
			{
				inProgressQuests.Add(quest);
			}
		}
			
		// If it has subquests, recurse into the first child
		if (quest.subQuests != null && quest.subQuests.Length > 0)
		{
			return MarkFirstChildBranchInProgress(quest.subQuests[0]);
		}

		// Leaf node
		return quest;
	}
}
