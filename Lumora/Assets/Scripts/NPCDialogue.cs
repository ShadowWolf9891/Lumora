using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField]
    int chapter;
    [SerializeField]
    int scene;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        GameContext.Instance.RaisePlayDialogue(chapter, scene);
    }
}
