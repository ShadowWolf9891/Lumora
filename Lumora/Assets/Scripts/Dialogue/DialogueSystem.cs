using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{

    private static DialogueSystem Instance;
   
    [SerializeField]
    Dictionary<string, Image[]> CharacterPortraits;

	DialogueData data; //All dialogue json file
    DialogueLine[] currentDialogue; //The current chapter / scene dialogue
	int currentLine = 0; //The current line in the dialogue
    string currentDialogueID;
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
		GameEvents<DialogueEvent>.Subscribe(BeginDialogue);
        //GameContext.Instance.OnPlayDialogue += BeginDialogue;
        GameEvents<PlayerInputEvent>.Subscribe(e => 
        {
            if (e.ActionType == PlayerInputActionType.NextDialogue) //Only check if the player presses the next dialogue button
            {
                NextLine();
            }
        }
        );
	}
	private void OnDisable()
	{
		GameEvents<DialogueEvent>.Unsubscribe(BeginDialogue);
		//GameContext.Instance.OnPlayDialogue += BeginDialogue;
		GameEvents<PlayerInputEvent>.Unsubscribe(e =>
		{
			if (e.ActionType == PlayerInputActionType.NextDialogue) //Only check if the player presses the next dialogue button
			{
				NextLine();
			}
		}
		);
	}
	private void Load()
    {
		TextAsset jsonFile = Resources.Load<TextAsset>("dialogue");
		data = JsonConvert.DeserializeObject<DialogueData>(jsonFile.text);
		Debug.Log($"Loaded dialogue json file.");
	}

    /// <summary>
    /// Get the dialogue lines for a specific chapter and scene.
    /// </summary>
    /// <param name="ChapterID">Which chapter of the story the dialogue takes place at</param>
    /// <param name="SceneID">Which scene within the chapter to play</param>
    /// <returns>An array of the dialogue lines that contains a speaker and what was said</returns>
    private DialogueLine[] GetDialogueLines(int ChapterID, int SceneID)
    {
        if(data == null)
        {
            Load();
        }

        foreach (var chapter in data.chapters)
        {
            if(chapter.id == ChapterID)
            {
                foreach(var scene in chapter.scenes)
                {
                    if (scene.id == SceneID) 
                    {
                        return scene.dialogues;
                    }
                }
            }
        }
        Debug.LogError($"Cannot find dialogue at chapter {ChapterID}, scene {SceneID}");
        return null;
    }
    /// <summary>
    /// Call this to start the dialogue for a specific chapter and scene. Does not control the player, NPC's or cinematics.
    /// </summary>
    /// <param name="ChapterID">The chapter to play the dialogue from</param>
    /// <param name="SceneID">The scene within the chapter to play the dialogue from</param>
    private void BeginDialogue(DialogueEvent e)
    {
        currentLine = 0;
        currentDialogue = GetDialogueLines(e.Chapter, e.Scene);
		UIManager.Instance.DisplayDialogue(currentDialogue[currentLine]);
        currentDialogueID = e.Id;
        EventManager.Instance.Raise("Pause_For_Dialogue");
	}
    
    /// <summary>
    /// Progress to the next line of dialogue. End the dialogue if there is no line to progress to.
    /// </summary>
    private void NextLine()
    {
        if (GameManager.Instance.CurrentGameState == GameStates.Dialogue)
        {
            if (currentLine < currentDialogue.Length - 1)
            {
                currentLine++;
				UIManager.Instance.DisplayDialogue(currentDialogue[currentLine]);
            }
            else
            {
                EndDialogue();
            }
        }
    }
    /// <summary>
    /// End the dialogue by reseting values and hiding the dialogue panel.
    /// </summary>
    private void EndDialogue() 
    {
        EventManager.Instance.MarkEventCompleted(currentDialogueID);
        UIManager.Instance.ClearUIText("DialogueElement");
        currentLine = 0;
        currentDialogue = null;
        currentDialogueID = "";
		CameraManager.Instance.SetCurrentCamera("3rd Person Camera", 0f);
		EventManager.Instance.Raise("Resume_Game");
	}

    
}
