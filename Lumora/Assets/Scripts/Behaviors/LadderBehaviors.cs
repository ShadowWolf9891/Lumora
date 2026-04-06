using UnityEngine;

public class LadderBehaviors : MonoBehaviour, IInteractable
{
    [Header("Ladder Top/Bottom Transforms")]
    [SerializeField] Transform topLadderPoint;
    [SerializeField] Transform bottomLadderPoint;

    [Header("Display variables")]
    [SerializeField]
    string interactionPrompt;
    [SerializeField]
    GameObject interactionPromptImageObjectTop, interactionPromptImageObjectBot;

    private GameObject playerRef;

    public bool isInteractionPromptVisible { get; private set; }

    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
    }
    public string GetInteractionPrompt()
    {
        isInteractionPromptVisible = true;
        if (IsPlayerCloserToTopPoint(playerRef))
        {
            if (interactionPromptImageObjectBot.activeSelf)
                interactionPromptImageObjectBot.SetActive(false);
            interactionPromptImageObjectTop.SetActive(true);
        }
        else
        {
            if (interactionPromptImageObjectTop.activeSelf)
                interactionPromptImageObjectTop.SetActive(false);
            interactionPromptImageObjectBot.SetActive(true);
        }

        return interactionPrompt;
    }
    public void DisableInteractionPrompt()
    {
        isInteractionPromptVisible = false;
        interactionPromptImageObjectTop.SetActive(false);
        interactionPromptImageObjectBot.SetActive(false);
    }
    public bool IsHoldInteraction()
    {
        return false;
    }

    public bool OnInteractStart()
    {
        Debug.Log("Running Interaction start with ladder");
        //Is top point or bottom point closer? teleport player to opposite point
        if (IsPlayerCloserToTopPoint(playerRef))
        {
            Debug.Log("LadderBehaviors: player is closer to TOP ladder point, starting TP event");
            GameEvents<TeleportPlayerEvent>.Raise(new TeleportPlayerEvent(
                $"",
                bottomLadderPoint.position));

        }
        else
        {
            Debug.Log("LadderBehaviors: player is closer to BOTTOM ladder point, starting TP event");
            GameEvents<TeleportPlayerEvent>.Raise(new TeleportPlayerEvent(
                $"",
                topLadderPoint.position));
        }

        //returning false, as this interaction doesn't destroy the ladder
        return false;
    }

    public void OnInteractStop()
    {
        //Null
    }

    private bool IsPlayerCloserToTopPoint(GameObject playerRef)
    {
        Vector3 distanceToTop = topLadderPoint.transform.position - playerRef.transform.position;
        Vector3 distanceToBot = bottomLadderPoint.transform.position - playerRef.transform.position;
        if (distanceToTop.magnitude < distanceToBot.magnitude)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
