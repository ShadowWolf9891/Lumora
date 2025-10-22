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
        WaypointManager.GetComponent<WaypointManager>().LoadWaypoint(0);
        GameEvents<StartQuestEvent>.Raise(new StartQuestEvent("TestQuest"));
		GameEvents<ProgressQuestEvent>.Raise(new ProgressQuestEvent("AllQuests"));
	}

    // Update is called once per frame
    void Update()
    {
		WaypointManager.GetComponent<WaypointManager>().UpdateDistance(playerRef);
    }
}
