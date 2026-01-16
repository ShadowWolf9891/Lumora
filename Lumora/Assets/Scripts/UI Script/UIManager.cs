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

    [SerializeField] private GameObject playerCanvasUI,pauseMenuUI, optionsMenuUI, controlMenuUI, dialogueMenuUI;
    private InputAction menuAction;
    [SerializeField]bool isMenuActive;

    void Start()
    {
        menuAction = InputSystem.actions.FindAction("Escape");
        isMenuActive = false;
        GameEvents<SpawnPauseMenuEvent>.Subscribe(OnMenuAction);
    }
    void Update()
    {
        if(!dialogueMenuUI.activeSelf){

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
                    GameEvents<SpawnPauseMenuEvent>.Raise(new SpawnPauseMenuEvent("Toggle_PauseMenu_On", isMenuActive));
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
            GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Pause_Game", GameStates.Paused)); 
        }
        else
        {
            GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Resume_Game", GameStates.Running));
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
