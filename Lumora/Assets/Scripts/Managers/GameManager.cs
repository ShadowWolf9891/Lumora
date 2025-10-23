using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerRef;
    [SerializeField] GameObject WaypointManager;

	private void Awake()
	{
        //The game manager subscribes the quest events to the static quest manager
		GameEvents<StartQuestEvent>.Subscribe(e => QuestManager.StartQuest(e.Id));
		GameEvents<ProgressQuestEvent>.Subscribe(e => QuestManager.ProgressQuest(e.Id));
	}
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameEvents<StartQuestEvent>.Raise(new StartQuestEvent("TestQuest"));
        //EventDispatcher.DispatchForCurrentQuest("SubQuest1");
       
	}

    // Update is called once per frame
    void Update()
    {
		WaypointManager.GetComponent<WaypointManager>().UpdateDistance(playerRef);
	}
}
