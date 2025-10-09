using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameContext.Instance.RaisePlayDialogue(2, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
