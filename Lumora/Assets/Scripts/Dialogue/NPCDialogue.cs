using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField]
    string dialogueID;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        EventManager.Raise(dialogueID);
      // GameEvents<DialogueEvent>.Raise(new DialogueEvent(chapter, scene));
        
        //GameContext.Instance.RaisePlayDialogue(chapter, scene);
    }

    private void OnTriggerExit(Collider other)
    {
        Destroy(gameObject);
    }
}
