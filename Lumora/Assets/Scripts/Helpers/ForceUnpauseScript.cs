using UnityEngine;

public class ForceUnpauseScript : MonoBehaviour
{
    [SerializeField]
    float timeBeforeUnpause;

    private void Start()
    {
        Invoke("TriggerUnpauseEvent", timeBeforeUnpause);
    }

    public void TriggerUnpauseEvent()
    {
        GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Unpausing Game", GameStates.Running));
    }
}
