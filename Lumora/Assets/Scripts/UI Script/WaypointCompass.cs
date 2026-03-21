using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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
        
        direction.y = 0;

        Vector3 camPos = cameraTransform.forward;
        camPos.y = 0;
        
        //Gets the horizontal angle between camera direction and waypoint
        float angle = Vector3.SignedAngle(camPos, direction, Vector3.up);
        Debug.Log(angle);

        //converts angle to UI pos
        float compassWidth = 500f;
        float normalized = angle / 180f;
        float halfWidth = compassWidth / 2;

        float xPos = normalized * halfWidth;
        
        float clampEnds = Mathf.Clamp(xPos, -halfWidth, halfWidth);
        
        
        //moves waypoint icon
        waypointIcon.rectTransform.anchoredPosition = new Vector2(clampEnds, waypointIcon.rectTransform.anchoredPosition.y);
    }
}
