using UnityEngine;

public class CollectibleBehaviour : MonoBehaviour, IInteractable
{
    [SerializeField]
    string interactionPrompt;
    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public bool IsHoldInteraction()
    {
        return false;
    }

    public void OnInteractStart()
    {
        Destroy(this);
    }

    public void OnInteractStop()
    {
        
    }

}
