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
        GameObject playerRef = GameObject.FindWithTag("Player");
        //Is top point or bottom point closer? teleport player to opposite point
        if (IsPlayerCloserToTopPoint(playerRef))
        {
            //Raise Event Tp player top
        }
        else
        {
            //Raise Event TP player bot
        }

        //Teleporting player will be an event raised by this behavior that player listens for.
        //upon hearing event, players will disable their model, begin movement, emit particles as they travel, then re-enable model at end point 

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
