using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenuUI, optionsMenuUI;
    private InputAction menuAction;
    [SerializeField]bool isMenuActive;
    bool eventActive;

    void Start()
    {
        menuAction = InputSystem.actions.FindAction("Escape");
        
        GameEvents<SpawnPauseMenuEvent>.Subscribe(OnMenuAction);
    }
    void Update()
    {
        if (menuAction.WasPressedThisFrame())
        {
            GameEvents<SpawnPauseMenuEvent>.Raise(new SpawnPauseMenuEvent("Toggle_PauseMenu_On", isMenuActive));
            if (optionsMenuUI.activeSelf)
            {
                optionsMenuUI.SetActive(false);
            }
        }
    }
    public void OnMenuAction(SpawnPauseMenuEvent a)
    {
        isMenuActive = !isMenuActive;
        pauseMenuUI.SetActive(isMenuActive);
        if (isMenuActive) 
        {
            GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Pause_Game", GameStates.Paused)); 
            //Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Resume_Game", GameStates.Running));
            //Cursor.lockState = CursorLockMode.Locked;
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
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }
    public void OnExitGame()
    {
        Application.Quit();
    }
    public void OnOptionsReturn()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
}
