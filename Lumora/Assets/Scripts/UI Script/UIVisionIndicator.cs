using System;
using System.Collections.Generic;
using UnityEngine;

public class UIVisionIndicator : MonoBehaviour
{
    [SerializeField]
    GameObject attachedGraphic;

    private List<GameObject> alertedEnemies;
    private void Start()
    {
        GameEvents<PlayerSpottedEvent>.Subscribe(LogEnemy);
        GameEvents<EnemyDropsAlert>.Subscribe(DeLogEnemy);
        alertedEnemies = new List<GameObject>();
    }

    private void LogEnemy(PlayerSpottedEvent e)
    {
        if (!alertedEnemies.Contains(e.Spotter))
        {
            alertedEnemies.Add(e.Spotter);
        }
        DoGraphicLogic();
    }

    private void DeLogEnemy(EnemyDropsAlert e)
    {
        if (alertedEnemies!= null && alertedEnemies.Contains(e.Enemy))
        {
            alertedEnemies.Remove(e.Enemy);
        }
        DoGraphicLogic();
    }
    private void DoGraphicLogic()
    {
        //this is where we can customize the graphic for alerted enemies. alert bar filling based on enemy awareness?
        if (!attachedGraphic.activeInHierarchy && alertedEnemies.Count > 0)
        {
            attachedGraphic.SetActive(true);
        }
        else if (attachedGraphic.activeInHierarchy && alertedEnemies.Count < 1)
        {
            attachedGraphic.SetActive(false);
        } 
    }
}
