using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerRef;
    [SerializeField] GameObject WaypointManager;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        WaypointManager.GetComponent<WaypointManager>().LoadWaypoint(0);
    }

    // Update is called once per frame
    void Update()
    {
		WaypointManager.GetComponent<WaypointManager>().UpdateDistance(playerRef);
    }
}
