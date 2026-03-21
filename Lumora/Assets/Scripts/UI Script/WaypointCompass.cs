using UnityEngine;

public class WaypointCompass : MonoBehaviour
{
    private GameObject playerRef;
    private GameObject activeWaypoint;
    

    void Awake()
    {
        playerRef = GameObject.Find("Player");
        activeWaypoint = playerRef.GetComponent<PlayerBehavior>().waypointImage;
    }
    void Update()
    {
        
    }
}
