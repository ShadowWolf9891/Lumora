using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class UIElement
{
    public string UI_Name;
    public GameObject UI_Prefab;
    public List<BindableChildren> bindableChildrenList;
}
[Serializable]
public class BindableChildren
{
	public string ChildToBindName;
	public UnityEvent OnClick;
	public StringEvent OnInputEndEdit;
}
[Serializable]
public class StringEvent : UnityEvent<string> { }

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] UIElement[] UIElements;
    private readonly HashSet<string> _loadedPrefabs = new();
    private readonly Dictionary<string, GameObject> _cachedObjects = new();
    private Canvas UICanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);

        UICanvas = GetComponentInChildren<Canvas>();
        OnNewSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    private void OnEnable()
    {
        GameEvents<UpdateUIEvent>.Subscribe(HandleUIVisibility);
        GameEvents<ChangeGameStateEvent>.Subscribe(HandleStateChange);
        SceneManager.sceneLoaded += OnNewSceneLoad;
    }
    private void OnDisable()
    {
        GameEvents<UpdateUIEvent>.Unsubscribe(HandleUIVisibility);
        GameEvents<ChangeGameStateEvent>.Unsubscribe(HandleStateChange);
        SceneManager.sceneLoaded -= OnNewSceneLoad;
    }
    private void OnNewSceneLoad(Scene newScene, LoadSceneMode loadSceneMode)
    {
        if (loadSceneMode != LoadSceneMode.Additive)
        {
            foreach (var obj in _cachedObjects)
            {
                HandleUIVisibility(obj.Key, false, false);
            }
            HandleUIVisibility("DialogueElement", false, false);
        }
        switch (newScene.name)
        {
            case "MainMenu":
                HandleUIVisibility("MainMenuElement", true, false);
                Cursor.lockState = CursorLockMode.None;
                break;
            case "Chapter1-Mine":
                HandleUIVisibility("PlayerElement", true, false);
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case "Chapter1-House":
                HandleUIVisibility("PlayerElement", true, false);
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case "Chapter2_Stealth":
                HandleUIVisibility("PlayerElement", true, false);
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case "":
                break;

        }
    }
    private void HandleUIVisibility(UpdateUIEvent e) => HandleUIVisibility(e.UI_Name, e.IsActive, e.LayerOnTop);
    private void HandleUIVisibility(string uiName, bool isVisible, bool layerOnTop)
    {
        if (_loadedPrefabs.Contains(uiName))
        {
            if (_cachedObjects[uiName].activeInHierarchy != isVisible) _cachedObjects[uiName].SetActive(isVisible);
            if (!layerOnTop && isVisible)
            {
                foreach (var kvp in _cachedObjects)
                {
                    if (kvp.Key != uiName) kvp.Value.gameObject.SetActive(false);
                }
            }
            return;
        }
        UIElement element = UIElements.FirstOrDefault(x => x.UI_Name == uiName);
        if (element.UI_Prefab == null)
        {
            Debug.LogError($"Invalid UI element with name {uiName}. Make sure it is spelled correctly and the prefab is loaded in the UIManager.");
            return;
        }
        GameObject temp = Instantiate(element.UI_Prefab, UICanvas.transform);
        if (element.bindableChildrenList != null && element.bindableChildrenList.Count > 0) BindElement(element, temp);
		_cachedObjects.Add(element.UI_Name, temp);
        _loadedPrefabs.Add(element.UI_Name);
    }
	private void HandleStateChange(ChangeGameStateEvent e)
	{
        if(SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "0_Bootstrap") return;
        switch (e.State)
        {
            case GameStates.Running:
				Cursor.lockState = CursorLockMode.Locked;
				HandleUIVisibility("PlayerElement", true, false);
                break;
            case GameStates.Paused:
				Cursor.lockState = CursorLockMode.None;
                HandleUIVisibility("PlayerElement", true, false); //Clear everything not on the UI canvas
                HandleUIVisibility("PauseElement", true, true); //Layer pause screen on top
				break;
            case GameStates.Dialogue:
				HandleUIVisibility("DialogueElement", true, false); //Clear everything except dialogue
				HandleUIVisibility("PlayerElement", false, false);
				break;
            case GameStates.Cutscene:
				HandleUIVisibility("DialogueElement", false, false); //Dialogue will always show when in a cutscene. Can change this.
				HandleUIVisibility("PlayerElement", false, false);
                break;
            case GameStates.Game_Over:
				Cursor.lockState = CursorLockMode.None;
				HandleUIVisibility("GameOverElement", true, false);
                break;
            case GameStates.Console:
				Cursor.lockState = CursorLockMode.None;
                HandleUIVisibility("ConsoleElement", true, false);
				break;
            default:
                break;

        }
	}

   
    private void BindElement(UIElement element, GameObject instance)
	{
        foreach (var childBind in element.bindableChildrenList)
        {
            var child = instance.transform.Find(childBind.ChildToBindName);
            if (child == null) continue;

            if (child.TryGetComponent<Button>(out var button))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => childBind.OnClick?.Invoke());
            }
            else if (child.TryGetComponent<TMP_InputField>(out var input))
            {
                input.onEndEdit.RemoveAllListeners();
                input.onEndEdit.AddListener((value) => childBind.OnInputEndEdit?.Invoke(value));
            }
        }
	}
    //Button behaviors on click
    public void OnStartClick() => EventManager.Instance.Raise(new LoadSceneEvent("StartNewGame", SceneManager.GetActiveScene().buildIndex + 2));
    public void OnResumeClick() => EventManager.Instance.Raise("Resume_Game");
	public void OnExitClick() => Application.Quit();
	public void OnOptionsPressed() =>  HandleUIVisibility("OptionElement", true, false);
    public void OnControllerPress() => HandleUIVisibility("ControllerElement", true, false);

    //New Start click,between 2 different illustrations (Main Menu Update)
    public void OnMainMenuStartClick()
    {
        if (!_cachedObjects.TryGetValue("MainMenuElement", out GameObject mainMenu)) return;

        var transition = mainMenu.GetComponent<MainMenuTransition>();

        foreach (Transform child in mainMenu.transform)
        {
            var cg = child.GetComponent<CanvasGroup>();
            if (cg == null) continue;

            string n = child.name.Trim();
            if (n == "Illustration1_Group") transition.illustration1 = cg;
            else if (n == "Illustration2_Group") transition.illustration2 = cg;
            else if (n == "LumoraTitle1") transition.lumoraTitle1 = cg;
            else if (n == "LumoraTitle2") transition.lumoraTitle2 = cg;
            else if (n == "ButtonsGroup") transition.buttonsGroup = cg;
            else if (n == "BlackOverlay") transition.blackOverlay = cg;
        }

        Debug.Log($"ill1={transition.illustration1} ill2={transition.illustration2}");
        transition.OnStartButtonPressed();
    }
    public void OnConsoleValueSubmit(string _)
    {
        if (!_loadedPrefabs.Contains("ConsoleElement")) return;

        string command = _cachedObjects["ConsoleElement"].GetComponentInChildren<TMP_InputField>().text;
		var textField = _cachedObjects["ConsoleElement"].GetComponentInChildren<ScrollRect>().content.GetComponentInChildren<TextMeshProUGUI>();
       
        string errorOrComplete = ConsoleWindow.DoConsoleCommand(command);
        textField.text = $"{textField.text}\n{errorOrComplete}";
        HandleUIVisibility("ConsoleElement", false, false);
		EventManager.Instance.Raise("Resume_Game");
	}
	/// <summary>
	/// Display the dialogue line on the screen and show the dialogue panel if it is hidden.
	/// </summary>
	/// <param name="line">The data that stores the speaker and what they are saying</param>
	public void DisplayDialogue(DialogueLine line)
	{
        if (!_cachedObjects.TryGetValue("DialogueElement", out GameObject dialoguePanel))
        {
            HandleUIVisibility("DialogueElement", true, true);
            dialoguePanel = _cachedObjects["DialogueElement"];
		}
        var textFields = dialoguePanel.GetComponentsInChildren<TextMeshProUGUI>(true).ToList();
        foreach ( var tf in textFields ) 
        {
            if (tf == null) continue;
            if (tf.name == "Txt_Name") tf.text = line.speaker;
			if (tf.name == "Txt_Dialogue") tf.text = line.text;
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
