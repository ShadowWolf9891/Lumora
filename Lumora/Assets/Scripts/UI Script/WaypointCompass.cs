using System;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

public class WaypointCompass : MonoBehaviour
{
    [SerializeField]private GameObject playerRef;
    [SerializeField]private GameObject activeWaypoint;
    [SerializeField]private Image waypointIcon;
    private TextMeshProUGUI iconText;
    private GameObject cameraRef;

    void Awake()
    {
        cameraRef = GameObject.Find("3rd Person Camera");
        playerRef = GameObject.Find("Player");
        activeWaypoint = playerRef.GetComponent<PlayerBehavior>().waypointImage;
        waypointIcon = this.GetComponent<Image>();
    }
    void Update()
    {
        UpdateCompass();
    }
    public void UpdateCompass()
    {
        Transform waypointTransform = activeWaypoint.transform;
        Transform playerTransform = playerRef.transform;
        Transform cameraTransform = cameraRef.transform;
        Vector3 direction = waypointTransform.position - playerTransform.position;

        //Gets the horizontal angle between camera direction and waypoint
        float angle = Vector3.SignedAngle(cameraTransform.forward, direction, Vector3.up);

        //convers angle it UI pos
        float compassWidth = 500f;
        float normalized = angle / 180;

        float xPos = normalized * (compassWidth / 2f);

        //moves waypoint icon
        waypointIcon.rectTransform.anchoredPosition = new Vector2(xPos, waypointIcon.rectTransform.anchoredPosition.y);
    
        //shows distance to waypoint
        float distance = Vector3.Distance(playerTransform.position, waypointTransform.position);
        //iconText.text = Mathf.RoundToInt(distance) + "m";

        //checks if waypoint is behind the player
        float dot = Vector3.Dot(playerTransform.forward, direction.normalized);
        if(dot < 0)
        {
            //if waypoint is behind player
            waypointIcon.gameObject.SetActive(false);
        }
        else
        {
            waypointIcon.gameObject.SetActive(true);
        }
    }
}
