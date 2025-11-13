using System;
using UnityEngine;

public class UIGameOverPanel : MonoBehaviour
{
    [SerializeField]
    GameObject gameOverPanel;
    void Start()
    {
        GameEvents<ChangeGameStateEvent>.Subscribe(OnGameStateChange);   
    }

    private void OnGameStateChange(ChangeGameStateEvent e)
    {
        if(e.State == GameStates.Game_Over)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
