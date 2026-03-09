using UnityEngine;

/// <summary>
/// Obsolete!!
/// </summary>
public class NPCDialogue : MonoBehaviour
{
    [SerializeField]
    string dialogueID;

    private void OnTriggerEnter(Collider other)
    {
        //if(!hasFired) EventManager.Instance.Raise(dialogueID);
        
      // GameEvents<DialogueEvent>.Raise(new DialogueEvent(chapter, scene));
        
        //GameContext.Instance.RaisePlayDialogue(chapter, scene);
    }

    private void OnTriggerExit(Collider other)
    {
        //Destroy(gameObject);
    }
}
