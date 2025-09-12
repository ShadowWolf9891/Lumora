using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    DialogueEvent dialogueEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueEvent.Raise(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
