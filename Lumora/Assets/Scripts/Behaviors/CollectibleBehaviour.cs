using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleBehaviour : MonoBehaviour, IInteractable
{
    [Header("Collectable Values")]
    [SerializeField]
    COLLECTABLE_TYPES collectableType;
    [SerializeField]
    int value;

    [Header("Display variables")]
    [SerializeField]
    string interactionPrompt;
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
        GameEvents<CollectionEvent>.Raise(new CollectionEvent($"Collection Event: {collectableType}, {value}", collectableType, value));
        Destroy(this.gameObject);
    }

    public void OnInteractStop()
    {
        
    }

}
