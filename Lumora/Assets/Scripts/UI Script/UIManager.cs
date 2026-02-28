using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    [SerializeField] private GameObject playerCanvasUI, pauseMenuUI, optionsMenuUI, controlMenuUI, dialogueMenuUI, consoleMenuUI;
    private InputAction menuAction, consoleAction;
    [SerializeField]GameObject currentCanvas;

	private void Awake()
	{
		if (instance == null) 
        { 
            instance = this; 
            DontDestroyOnLoad(gameObject);
        }
		else Destroy(this);
	}
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
    private void OnMenuAction(SpawnPauseMenuEvent a)
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
        EventManager.MarkEventCompleted(a.Id);
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
    public void OnConsoleValueSubmit()
    {
        string command = consoleMenuUI.GetComponentInChildren<TMP_InputField>().text;
		var textField = consoleMenuUI.GetComponentInChildren<ScrollRect>().content.GetComponentInChildren<TextMeshProUGUI>();
       
        string errorOrComplete = ConsoleWindow.DoConsoleCommand(command);
        textField.text = $"{textField.text}\n{errorOrComplete}";
        consoleMenuUI.SetActive(false);
		Cursor.lockState = CursorLockMode.Locked;

	}
}
