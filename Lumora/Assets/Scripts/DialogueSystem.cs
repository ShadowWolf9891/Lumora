using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{

    [Header("UI Hookup"),SerializeField]
    GameObject DialoguePanel;
    [SerializeField]
    TextMeshProUGUI SpeakerName;
    [SerializeField]
    TextMeshProUGUI DialogueText;

	[Header("Dialogue Event Reference"), SerializeField] private DialogueEvent dialogueEvent;
	void OnEnable() => dialogueEvent.Register(BeginDialogue);
	void OnDisable() => dialogueEvent.Unregister(BeginDialogue);

	DialogueData data; //All dialogue json file
    DialogueLine[] currentDialogue; //The current chapter / scene dialogue
	int currentLine = 0; //The current line in the dialogue

    void Load()
    {
		TextAsset jsonFile = Resources.Load<TextAsset>("dialogue");
		data = JsonUtility.FromJson<DialogueData>(jsonFile.text);
		Debug.Log($"Loaded json file.");
	}

    /// <summary>
    /// Get the dialogue lines for a specific chapter and scene.
    /// </summary>
    /// <param name="ChapterID">Which chapter of the story the dialogue takes place at</param>
    /// <param name="SceneID">Which scene within the chapter to play</param>
    /// <returns>An array of the dialogue lines that contains a speaker and what was said</returns>
    public DialogueLine[] GetDialogueLines(int ChapterID, int SceneID)
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
    public void BeginDialogue(int ChapterID, int SceneID)
    {

        currentLine = 0;
        currentDialogue = GetDialogueLines(ChapterID, SceneID);
		DisplayDialogue(currentDialogue[currentLine]);
	}
    /// <summary>
    /// Display the dialogue line on the screen and show the dialogue panel if it is hidden.
    /// </summary>
    /// <param name="line">The data that stores the speaker and what they are saying</param>
    public void DisplayDialogue(DialogueLine line)
    { 
        SpeakerName.text = line.speaker;
		DialogueText.text = line.text;
        if(!DialoguePanel.activeInHierarchy) DialoguePanel.SetActive(true);
	}
    /// <summary>
    /// Progress to the next line of dialogue. End the dialogue if there is no line to progress to.
    /// </summary>
    public void NextLine()
    {
        if (DialoguePanel.activeInHierarchy)
        {
            if (currentLine < currentDialogue.Length - 1)
            {
                currentLine++;
                DisplayDialogue(currentDialogue[currentLine]);
            }
            else
            {
                EndDialogue();
            }
        }
        else
        {
            Debug.LogError("Tried to progress to the next line but the dialogue panel is not visible.");
        }

    }
    /// <summary>
    /// End the dialogue by reseting values and hiding the dialogue panel.
    /// </summary>
    public void EndDialogue() 
    {
        DialoguePanel.SetActive(false);
		SpeakerName.text = "";
		DialogueText.text = "";
        currentLine = 0;
        currentDialogue = null;
	}

    
}
