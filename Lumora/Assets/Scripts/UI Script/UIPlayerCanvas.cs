using System.Collections.Generic;
using UnityEngine;

public class UIPlayerCanvas : MonoBehaviour
{    
    public static UIPlayerCanvas Instance;
    
    [Header("Control UI")]
    public Transform controllerLayoutGroup;

    //control icon prefabs
    public List<GameObject> constControls;
    public List<GameObject> runStateControls;
    public List<TriggerControls> triggerControls;
    public List<GameObject> eventControls;

    public List<GameObject> currentUI = new List<GameObject>();
    public string currentLayer;
    void Start()
    {
        if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);
    }
    void OnEnable()
    {
        RefreshUI();
    }
    void OnDisable()
    {
        ClearUI();
    }
    public void RefreshUI()
    {
        ClearUI();

        foreach(var control in constControls) AddControl(control);

        //running game state control icons
        if (GameManager.Instance.CurrentGameState == GameStates.Running)
        {
            foreach (var control in runStateControls) AddControl(control);
        }

        //player collider dependant icons
        foreach (var t in triggerControls)
        {
            if(t.layer.ToString() == currentLayer)
            {
                AddControl(t.prefab);
            }
        }

        currentLayer = null;
    }
    public void AddControl(GameObject controlToAdd)
    {
        GameObject obj = Instantiate(controlToAdd, controllerLayoutGroup);
        currentUI.Add(obj);

    }
    public void ClearUI()
    {
        foreach (var obj in currentUI)
        {
            Destroy(obj);
        }
        currentUI.Clear();
    }
    public void TriggerControl(LayerMask layer)
    {
        currentLayer = layer.ToString();
    }
}
[System.Serializable]
public class TriggerControls
{
    public GameObject prefab;
    public LayerMask layer;
}
