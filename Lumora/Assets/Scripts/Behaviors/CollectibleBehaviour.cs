using UnityEngine;
using UnityEngine.UI;

public class CollectibleBehaviour : MonoBehaviour, IInteractable
{
    [SerializeField]
    string interactionPrompt;
    [SerializeField]
    Sprite interactionPromptImage;
    [SerializeField]
    GameObject interactionPromptImageObject;
    public string GetInteractionPrompt()
    {
        interactionPromptImageObject.SetActive(true);
        return interactionPrompt;
    }

    public void PlayerLeavesInteractRange()
    {
        interactionPromptImageObject.SetActive(false);
    }

    public bool IsHoldInteraction()
    {
        return false;
    }

    public void OnInteractStart()
    {
        //DO COLLECTION BEHAVIOR HERE

        Destroy(this.gameObject);
    }

    public void OnInteractStop()
    {
        
    }

}
