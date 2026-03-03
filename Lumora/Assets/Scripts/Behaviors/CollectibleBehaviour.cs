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

    public bool isInteractionPromptVisible { get; private set; }
    public string GetInteractionPrompt()
    {
        if (!isInteractionPromptVisible)
        {
            isInteractionPromptVisible = true;
            interactionPromptImageObject.SetActive(true);
        }
        return interactionPrompt;
    }

    public void DisableInteractionPrompt()
    {
        isInteractionPromptVisible = false;
        if (interactionPromptImageObject != null)
        {
            interactionPromptImageObject.SetActive(false);
        }
    }

    public bool IsHoldInteraction()
    {
        return false;
    }

    public bool OnInteractStart()
    {
        GameEvents<CollectionEvent>.Raise(new CollectionEvent($"Collection Event: {collectableType}, {value}", collectableType, value));
        Destroy(this.gameObject);
        return true;
    }

    public void OnInteractStop()
    {
        
    }

}
