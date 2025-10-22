using TMPro;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private Transform[] waypoints;

    private GameObject curWaypoint;
    private int curIndex;
    public void LoadWaypoint(int index)
    {
        if (index >= waypoints.Length) return;
		
		Destroy(curWaypoint);
		curWaypoint = Instantiate(waypointPrefab, waypoints[index]);
        if (!curWaypoint.activeInHierarchy)
        {
			curWaypoint.SetActive(true);
        }
    }

    public void UpdateDistance(GameObject playerRef)
    {
        if(curWaypoint !=null &&  curWaypoint.activeInHierarchy) 
        {
            float distance = Vector3.Distance(playerRef.transform.position, curWaypoint.transform.position);
            curWaypoint.GetComponentInChildren<TextMeshProUGUI>().text = $"{(int)distance}m";
            if(distance < 1)
            {
                GoToNextWaypoint();
            }
        }
    }

    public void GoToNextWaypoint()
    {
        curIndex++;
        LoadWaypoint(curIndex);
    }

}
