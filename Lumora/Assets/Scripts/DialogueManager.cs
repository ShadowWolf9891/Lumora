using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    private InputAction nextLine;
    [SerializeField] DialogueSystem dialogueSystem;
    void Start()
    {
        nextLine = InputSystem.actions.FindAction("North");
    }
    void Update()
    {
        if (nextLine.WasPressedThisFrame())
        {
            dialogueSystem.NextLine();
        }
    }

}
