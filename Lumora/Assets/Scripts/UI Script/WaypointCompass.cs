using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaypointCompass : MonoBehaviour
{
    private GameObject playerRef;
    private GameObject cameraRef;
    private GameObject activeWaypoint;
    [SerializeField]private Image waypointIcon;
    
    private TextMeshProUGUI iconText;

    void Awake()
    {
        cameraRef = GameObject.Find("3rd Person Camera");
        playerRef = GameObject.Find("Player");
        activeWaypoint = playerRef.GetComponent<PlayerBehavior>().waypointImage;
        waypointIcon = GetComponent<Image>();
        iconText = GetComponentInChildren<TextMeshProUGUI>();
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
        
        direction.y = 0;

        Vector3 camPos = cameraTransform.forward / 2;
        camPos.y = 0;
        
        //Gets the horizontal angle between camera direction and waypoint
        float angle = Vector3.SignedAngle(camPos, direction, Vector3.up);

        //converts angle to UI pos
        float compassWidth = 500f;
        float normalized = angle / 60f;
        float halfWidth = compassWidth / 2;

        float xPos = normalized * halfWidth;
        
        float clampEnds = Mathf.Clamp(xPos, -halfWidth, halfWidth);
        
        
        //moves waypoint icon and updates text
        waypointIcon.rectTransform.anchoredPosition = new Vector2(clampEnds, waypointIcon.rectTransform.anchoredPosition.y);
        
        float distance = Vector3.Distance(playerTransform.position, waypointTransform.position);
        iconText.text = Mathf.RoundToInt(distance) + "m";
    }
}
