using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class QuestManager
{
    private static AllQuests data = null;
    public static void LoadQuests()
    {
		TextAsset jsonFile = Resources.Load<TextAsset>("quests");
		data = JsonUtility.FromJson<AllQuests>(jsonFile.text);
		Debug.Log($"Loaded json file.");
	}
   
    public static void CompleteQuest(string id)
    {
        if(data == null) { LoadQuests();}

        //Check top level quests, i.e. parentQuest is null
        foreach(var quest in data.questData)
        {
            if(CheckSubquests(id, quest))
            {
                //TODO: Write to Json file if the quest was found
                return;
			}
        }
    }

    private static bool CheckSubquests(string id, QuestData quest)
    { 
        if(quest.id == id) //Found quest 
        {
            quest.status = QuestStatus.COMPLETED;
            return true;
        }

        if(quest.subQuests == null) //No subquests to check
        {
            return false;
        }
		foreach (var subQuest in quest.subQuests)
		{
			if (CheckSubquests(id, subQuest)) //Recursively check subquests
                return true;
		}
        return false; //False if no subquests match id
	}
}
