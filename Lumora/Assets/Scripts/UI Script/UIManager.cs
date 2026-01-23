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

    [SerializeField] private GameObject playerCanvasUI, pauseMenuUI, optionsMenuUI, controlMenuUI, dialogueMenuUI, consoleMenuUI;
    private InputAction menuAction, consoleAction;
    [SerializeField]GameObject currentCanvas;

    void Start()
    {
        menuAction = InputSystem.actions.FindAction("Escape");
        consoleAction = InputSystem.actions.FindAction("Console");
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
                EventManager.Raise("Toggle_PauseMenu_On");
            }
        }
    }
    public void OnMenuAction(SpawnPauseMenuEvent a)
    {
        //opens pause menu
        if (currentCanvas == null)
        {
            playerCanvasUI.SetActive(false);
            currentCanvas = pauseMenuUI;
            pauseMenuUI.SetActive(true);
            EventManager.Raise("Pause_Game");
        }
        else
        {
            currentCanvas.SetActive(false);

            if (pauseMenuUI.activeSelf)
            {
                currentCanvas = pauseMenuUI;
            }
            else
            {
                playerCanvasUI.SetActive(true);
                currentCanvas = null;
                EventManager.Raise("Resume_Game");
            }
        }
    }

    //button controllers
    public void OnResumePressed()
    {
        EventManager.Raise("Toggle_PauseMenu_On");
    }
    public void OnOptionsPressed()
    {
        currentCanvas = optionsMenuUI;
        optionsMenuUI.SetActive(true);
    }
    public void OnControllerPress()
    {
        currentCanvas = controlMenuUI;
        controlMenuUI.SetActive(true);
    }
    public void OnExitGame()
    {
        Application.Quit();
    }
}
