using UnityEngine;

public class LadderBehaviors : MonoBehaviour, IInteractable
{
    [Header("Ladder Top/Bottom Transforms")]
    [SerializeField] Transform topLadderPoint;
    [SerializeField] Transform bottomLadderPoint;

    public string GetInteractionPrompt()
    {
        return ("Press E to use ladder.");
    }
    public void DisableInteractionPrompt()
    {

    }
    public bool IsHoldInteraction()
    {
        return false;
    }

    public bool OnInteractStart()
    {
        Debug.Log("Running Interaction start with ladder");
        GameObject playerRef = GameObject.FindWithTag("Player");
        //Is top point or bottom point closer? teleport player to opposite point
        if (IsPlayerCloserToTopPoint(playerRef))
        {
            Debug.Log("LadderBehaviors: player is closer to TOP ladder point, starting TP event");
            GameEvents<TeleportPlayerEvent>.Raise(new TeleportPlayerEvent(
                $"Teleporting Player to: {bottomLadderPoint} from: {topLadderPoint}", 
                bottomLadderPoint.position));

        }
        else
        {
            Debug.Log("LadderBehaviors: player is closer to BOTTOM ladder point, starting TP event");
            GameEvents<TeleportPlayerEvent>.Raise(new TeleportPlayerEvent(
                $"Teleporting Player to: {topLadderPoint} from: {bottomLadderPoint}", 
                topLadderPoint.position));
        }

        //Teleporting player will be an event raised by this behavior that player listens for.
        //upon hearing event, players will disable their model, begin movement, emit particles as they travel, then re-enable model at end point 

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
