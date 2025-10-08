using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    DialogueEvent dialogueEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //dialogueEvent.Raise(2, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
