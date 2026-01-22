using System;
using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject playerCanvasUI,pauseMenuUI, optionsMenuUI, controlMenuUI, dialogueMenuUI, consoleMenuUI;
    private InputAction menuAction, consoleAction;
    [SerializeField]bool isMenuActive;

    void Start()
    {
        menuAction = InputSystem.actions.FindAction("Escape");
        consoleAction = InputSystem.actions.FindAction("Console");
        isMenuActive = false;
        GameEvents<SpawnPauseMenuEvent>.Subscribe(OnMenuAction);
    }
    void Update()
    {
        if(!dialogueMenuUI.activeSelf){
            if (consoleAction.WasPressedThisFrame())
            {
                if (consoleMenuUI.activeSelf)
                {   
                    //closes console window
                    consoleMenuUI.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    EventManager.Raise("Resume_Game");
                }
                else if (!consoleMenuUI.activeSelf)
                {
                    //opens console window
                    consoleMenuUI.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    EventManager.Raise("Pause_Game");
                }
            }
            if (menuAction.WasPressedThisFrame())
            {            
                if (optionsMenuUI.activeSelf)
                {   
                    optionsMenuUI.SetActive(false);
                }
                else if (controlMenuUI.activeSelf)
                {
                    controlMenuUI.SetActive(false);
                }
                else
                {
                    EventManager.Raise("Toggle_PauseMenu_On");
                }
            }
        }
    }
    public void OnMenuAction(SpawnPauseMenuEvent a)
    {
        isMenuActive = !isMenuActive;
        pauseMenuUI.SetActive(isMenuActive);
        if (isMenuActive) 
        {
            playerCanvasUI.SetActive(false);
            EventManager.Raise("Pause_Game"); 
        }
        else
        {
            EventManager.Raise("Resume_Game");
            playerCanvasUI.SetActive(true);
        }
    }

    //button controller
    public void OnResumePressed()
    {
        isMenuActive = false;
        pauseMenuUI.SetActive(isMenuActive);
        //Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnOptionsPressed()
    {
        optionsMenuUI.SetActive(true);
    }
    public void OnOptionsReturn()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
    public void OnControlPress()
    {
        controlMenuUI.SetActive(true);
    }
    public void OnExitGame()
    {
        Application.Quit();
    }
}
