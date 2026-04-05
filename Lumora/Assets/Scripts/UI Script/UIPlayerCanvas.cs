using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerCanvas : MonoBehaviour
{    
    public static UIPlayerCanvas Instance;
    
    [Header("Health Bar")]
    [SerializeField]Sprite[] healthSprites;
    [SerializeField]Image healthIcon;


    [Header("Control UI")]
    public Transform controllerLayoutGroup;

    //control icon prefabs
    public List<GameObject> constControls;
    public List<GameObject> runStateControls;
    public List<TriggerControls> triggerControls;
    public List<GameObject> eventControls;

    public List<GameObject> currentUI = new List<GameObject>();
    public string currentLayer;

    
    [Header("Waypoint Compass")]
    private GameObject playerRef;
    private GameObject cameraRef;
    
    //Waypoints
    [SerializeField]private GameObject activeWaypoint;
    [SerializeField]private Image waypointIcon;
    
    private TextMeshProUGUI iconText;
    void Awake()
    {
        if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);

        cameraRef = GameObject.Find("3rd Person Camera");
        playerRef = GameObject.Find("Player");
        activeWaypoint = playerRef.GetComponent<PlayerBehavior>().waypointImage;
        iconText = GameObject.Find("T_Waypoint").GetComponentInChildren<TextMeshProUGUI>();
    }
    void Update()
    {
        UpdateCompass();
    }
    void OnEnable()
    {
        RefreshUI();
        activeWaypoint = playerRef.GetComponent<PlayerBehavior>().waypointImage;
    }
    void OnDisable()
    {
        ClearUI();
        activeWaypoint = null;
    }
    public void UpdateHealthBar(int currentHealth)
    {
        Debug.Log(currentHealth);
        if (currentHealth >= 7) //above 7
        {
            healthIcon.sprite = healthSprites[0];
        }
        else if (currentHealth <= 6 && currentHealth >= 4) //between 6 and 4
        {
            healthIcon.sprite = healthSprites[1];
        }
        else //below 3
        {
            healthIcon.sprite = healthSprites[2];
        }
    }
    public void RefreshUI()
    {
        ClearUI();

        foreach(var control in constControls) AddControl(control);

        //running game state control icons
        if (GameManager.Instance.CurrentGameState == GameStates.Running)
        {
            foreach (var control in runStateControls) AddControl(control);

            //player collider dependant icons
            foreach (var t in triggerControls)
            {
                if(t.layer.ToString() == currentLayer)
                {
                    AddControl(t.prefab);
                }
            }
        }


        currentLayer = null;
    }
    public void AddControl(GameObject controlToAdd)
    {
        GameObject obj = Instantiate(controlToAdd, controllerLayoutGroup);
        currentUI.Add(obj);

    }
    public void TriggerControl(LayerMask layer)
    {
        currentLayer = layer.ToString();
    }
    public void ClearUI()
    {
        foreach (var obj in currentUI)
        {
            Destroy(obj);
        }
        currentUI.Clear();
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

[System.Serializable]
public class TriggerControls
{
    public GameObject prefab;
    public LayerMask layer;
}
