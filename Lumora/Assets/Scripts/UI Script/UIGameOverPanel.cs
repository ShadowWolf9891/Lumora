using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIGameOverPanel : MonoBehaviour
{
    private InputAction restart;
    [SerializeField]
    GameObject gameOverPanel;
    void Start()
    {
        GameEvents<ChangeGameStateEvent>.Subscribe(OnGameStateChange);  
        restart = InputSystem.actions.FindAction("South"); 
    }
    void Update()
    {
        if (restart.WasPressedThisFrame() && gameOverPanel.activeSelf)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void OnGameStateChange(ChangeGameStateEvent e)
    {
        if(e.State == GameStates.Game_Over)
        {
            EventManager.Raise("Resume_Game");
            gameOverPanel.SetActive(true);
        }
    }
}
