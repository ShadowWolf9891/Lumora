using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class UIElement
{
    public string UI_Name;
    public GameObject UI_Prefab;
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] GameObject UICanvas;
    [SerializeField] List<UIElement> UIElements;
    private readonly HashSet<string> _loadedPrefabs = new();
    private readonly Dictionary<string,GameObject> _cachedObjects = new();
    private InputAction restartAction, consoleAction, backAction, pauseAction;

	private void Awake()
	{
		if (Instance == null) 
        { 
            Instance = this;
        }
		else Destroy(this);
	}
	private void OnEnable()
	{
		GameEvents<UpdateUIEvent>.Subscribe(HandleUIVisibility);
        GameEvents<ChangeGameStateEvent>.Subscribe(HandleStateChange);
        InputSystem.onActionChange += HandleButtonPress;
        SceneManager.sceneLoaded += OnNewSceneLoad;
	}

	private void OnDisable()
	{
		GameEvents<UpdateUIEvent>.Unsubscribe(HandleUIVisibility);
		GameEvents<ChangeGameStateEvent>.Unsubscribe(HandleStateChange);
	}
	private void Start()
	{
		restartAction = InputSystem.actions.FindAction("South");
		consoleAction = InputSystem.actions.FindAction("Console");
		backAction = InputSystem.actions.FindAction("East");
		pauseAction = InputSystem.actions.FindAction("Escape");
	}
	private void OnNewSceneLoad(Scene newScene, LoadSceneMode loadSceneMode)
	{
        if (newScene == SceneManager.GetSceneByName("MainMenu"))
        {
            HandleUIVisibility("MainMenuElement", true, false);
            Cursor.lockState = CursorLockMode.None;
        }
	}

	private void HandleUIVisibility(UpdateUIEvent e) => HandleUIVisibility(e.UI_Name, e.IsActive, e.LayerOnTop);
    private void HandleUIVisibility(string uiName, bool isVisible, bool layerOnTop)
    {
        if (_loadedPrefabs.Contains(uiName))
        {
            if(_cachedObjects[uiName].activeInHierarchy != isVisible) _cachedObjects[uiName].SetActive(isVisible);
            if (!layerOnTop && isVisible)
            {
                foreach (var kvp in _cachedObjects)
                {
                    if (kvp.Key != uiName) kvp.Value.gameObject.SetActive(false);
                }
            }
            return;
        }
        UIElement element = UIElements.Find(x => x.UI_Name == uiName);
        if (element == null || element.UI_Prefab == null)
        {
            Debug.LogError($"Invalid UI element with name {uiName}. Make sure it is spelled correctly and the prefab is loaded in the UIManager.");
            return;
        }
        _cachedObjects.Add(element.UI_Name, Instantiate(element.UI_Prefab, UICanvas.transform));
        _loadedPrefabs.Add(element.UI_Name);
    }

	private void HandleStateChange(ChangeGameStateEvent e)
	{
        switch (e.State)
        {
            case GameStates.Running:
				Cursor.lockState = CursorLockMode.Locked;
				HandleUIVisibility("PlayerElement", true, false);
                break;
            case GameStates.Paused:
				Cursor.lockState = CursorLockMode.None;
                if (!_cachedObjects["ConsoleElement"].activeInHierarchy) //If the console is not open
                {
                    HandleUIVisibility("PlayerElement", true, false); //Clear everything not on the UI canvas
                    HandleUIVisibility("PauseElement", true, true); //Layer pause screen on top
                }
				break;
            case GameStates.Dialogue:
				HandleUIVisibility("DialogueElement", true, false); //Clear everything except dialogue
				break;
            case GameStates.Cutscene:
				HandleUIVisibility("DialogueElement", true, false); //Dialogue will always show when in a cutscene. Can change this.
				break;
            case GameStates.Game_Over:
				Cursor.lockState = CursorLockMode.None;
				HandleUIVisibility("GameOverElement", true, false);
                break;
            default:
                break;

        }
	}
	private void HandleButtonPress(object inputAction, InputActionChange change)
    {
        if(change != InputActionChange.ActionStarted) return;

		if (inputAction == consoleAction)
		{
			HandleUIVisibility("ConsoleElement", !_cachedObjects["ConsoleElement"].activeInHierarchy, true);
			EventManager.Instance.Raise(_cachedObjects["ConsoleElement"].activeInHierarchy ? "Pause_Game" : "Resume_Game");
		}

		switch (GameManager.Instance.CurrentGameState)
        {
            case GameStates.Running:
                if (inputAction == pauseAction) EventManager.Instance.Raise("Pause_Game");
				break;
            case GameStates.Paused:
                if (inputAction == backAction || inputAction == pauseAction) OnResumePressed();
                break;
            case GameStates.Dialogue:
                //Skip dialogue stuff, or go to next line when their is no player
                break;
            case GameStates.Cutscene:
                //Skip cutscene stuff
                break;
            case GameStates.Game_Over:
				if (restartAction.WasPressedThisFrame())
				{
					EventManager.Instance.Raise(new LoadSceneEvent("ReloadThisScene", SceneManager.GetActiveScene().buildIndex));
				}
				break;
            default: break;
        }
    }
	//button controllers
	public void OnStartClick()
	{
		Debug.Log("Loading scene " + SceneManager.GetActiveScene().buildIndex + 1);
		EventManager.Instance.Raise(new LoadSceneEvent("StartNewGame", SceneManager.GetActiveScene().buildIndex + 1));
	}
	public void OnResumePressed()
    {
        EventManager.Instance.Raise("Resume_Game");
    }
    public void OnOptionsPressed()
    {
        HandleUIVisibility("OptionElement", true, false);
    }
    public void OnControllerPress()
    {
		HandleUIVisibility("ControllerElement", true, false);
	}
    public void OnExitGame()
    {
        Application.Quit();
    }
    public void OnConsoleValueSubmit()
    {
        if (!_loadedPrefabs.Contains("ConsoleElement")) return;

        string command = _cachedObjects["ConsoleElement"].GetComponentInChildren<TMP_InputField>().text;
		var textField = _cachedObjects["ConsoleElement"].GetComponentInChildren<ScrollRect>().content.GetComponentInChildren<TextMeshProUGUI>();
       
        string errorOrComplete = ConsoleWindow.DoConsoleCommand(command);
        textField.text = $"{textField.text}\n{errorOrComplete}";
		EventManager.Instance.Raise("Resume_Game");
	}
	/// <summary>
	/// Display the dialogue line on the screen and show the dialogue panel if it is hidden.
	/// </summary>
	/// <param name="line">The data that stores the speaker and what they are saying</param>
	public void DisplayDialogue(DialogueLine line)
	{
        var dialoguePanel = _cachedObjects["DialogueElement"];
        var textFields = dialoguePanel.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        foreach ( var tf in textFields ) 
        {
            if (tf == null) continue;
            if (tf.name == "Txt_Name") tf.text = line.speaker;
			if (tf.name == "Txt_Dialogue") tf.text = line.speaker;
		}
		//TODO: Add Image UI and initalize it

		if (line.cameraName != "" || line.cameraName != null)
		{
			CameraManager.Instance.SetCurrentCamera(line.cameraName, line.blendSpeed);
		}
	}
    public void ClearUIText(string UIElementName)
    {
        if(!_loadedPrefabs.Contains(UIElementName)) return;
        var textFields = _cachedObjects[UIElementName].GetComponentsInChildren<TextMeshProUGUI>().ToList();
        if(textFields == null || textFields.Count <= 0) return;
        foreach ( var tf in textFields ) tf.text = "";
	}
}
