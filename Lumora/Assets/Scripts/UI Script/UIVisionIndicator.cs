using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicatorPair
{
    public GameObject Enemy;
    public GameObject UIIndicatorGraphic;
    public EnemyIndicatorPair(GameObject enemy, GameObject uIIndicatorGraphic)
    {
        Enemy = enemy;
        UIIndicatorGraphic = uIIndicatorGraphic;
    }
}

public class UIVisionIndicator : MonoBehaviour
{
    [SerializeField]
    float circleRadius;
    [SerializeField]
    GameObject indicatorGraphicLayer;
    [SerializeField]
    GameObject graphicObjectToSpawn;
    [SerializeField]
    Transform canvasCenterPoint;

    private GameObject playerRef;
    private Transform cameraTransform;

    private List<GameObject> alertedEnemies;
    private List<EnemyIndicatorPair> graphicObjectList = new List<EnemyIndicatorPair>();
    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        cameraTransform = GameObject.FindGameObjectWithTag("Camera").transform;
        GameEvents<PlayerSpottedEvent>.Subscribe(LogEnemy);
        GameEvents<EnemyDropsAlert>.Subscribe(DeLogEnemy);
        alertedEnemies = new List<GameObject>();
    }
    private void Update()
    {
        if (graphicObjectList.Count > 0)
        {
            UpdateVisionIndicators();
        }
    }
    private void LogEnemy(PlayerSpottedEvent e)
    {
        if (!alertedEnemies.Contains(e.Spotter))
        {
            alertedEnemies.Add(e.Spotter);
            GameObject newGraphic = Instantiate(graphicObjectToSpawn, indicatorGraphicLayer.transform);
            graphicObjectList.Add(new EnemyIndicatorPair(e.Spotter, newGraphic));
        }
        DoGraphicLogic();
    }

    private void DeLogEnemy(EnemyDropsAlert e)
    {
        if (alertedEnemies!= null && alertedEnemies.Contains(e.Enemy))
        {
            int index = alertedEnemies.IndexOf(e.Enemy);
            alertedEnemies.Remove(e.Enemy);
            if (graphicObjectList[index].Enemy = e.Enemy)
            {
                Destroy(graphicObjectList[index].UIIndicatorGraphic);
                graphicObjectList.RemoveAt(index);
            }
            else
            {
                Debug.Log("UI VISION INDICATOR: this is the shit got fucky alert");
            }
        }
        DoGraphicLogic();
    }
    private void DoGraphicLogic()
    {
        if (!indicatorGraphicLayer.activeInHierarchy && alertedEnemies.Count > 0)
        {
            indicatorGraphicLayer.SetActive(true);
            UpdateVisionIndicators();
        }
        else if (indicatorGraphicLayer.activeInHierarchy && alertedEnemies.Count < 1)
        {
            indicatorGraphicLayer.SetActive(false);
        } 
    }

    private void UpdateVisionIndicators()
    {
        if (graphicObjectList.Count <= 0)
        {
            Debug.LogError("UI VISION INDICATORS: graphic objects list is empty, but update indicators was called");
            return;
        }
        foreach (EnemyIndicatorPair graphicPair in graphicObjectList)
        {
            graphicPair.UIIndicatorGraphic.transform.position = canvasCenterPoint.position + new Vector3(0, 150, 0);
            //Commented out for thursday build!
            //SetIndicatorRotation(graphicPair.UIIndicatorGraphic, graphicPair.Enemy);
        }

    }

    private void SetIndicatorRotation(GameObject indicatorGameOb, GameObject enemyGameOb)
    {
        Vector3 directionToEnemy = (playerRef.transform.position - enemyGameOb.transform.position).normalized;
        Vector3 directionToEnemy2D = new Vector3(directionToEnemy.x, directionToEnemy.z, 0);
        Vector3 camDirectionToPlayer = (playerRef.transform.position - cameraTransform.position).normalized;
        Vector3 camDirectionToPlayer2D = new Vector3(camDirectionToPlayer.x, camDirectionToPlayer.z, 0);
        Vector3 indicatorVectorPos = -(directionToEnemy2D - camDirectionToPlayer2D).normalized;
        Vector3 intendedPos =  canvasCenterPoint.position - (indicatorVectorPos * circleRadius);
        indicatorGameOb.transform.position = intendedPos;
        //based on x,y, rotate object to face center point
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(canvasCenterPoint.position, circleRadius);
    }
}
