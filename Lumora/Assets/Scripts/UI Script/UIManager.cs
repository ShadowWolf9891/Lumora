using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public InputActionAsset inputActions;
    public GameObject pauseCanvas, optionsCanvas, inventoryCanvas;
    private InputAction optionsAction;
    private InputAction inventoryAction;
    public static UIManager Instance;
    [SerializeField] GameObject activeCanvas;
    public GameObject endScreen;
    void Start()
    {
        //QuestManager.GetInProgress();
    }
    /*
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        optionsAction = InputSystem.actions.FindAction("Escape");
        inventoryAction = InputSystem.actions.FindAction("North");
    }
    void Update()
    {
        if (optionsAction.WasPressedThisFrame())
        {
            //checks if a canvas is active, if not open pause menu
            if (activeCanvas != null)
            {
                activeCanvas.SetActive(false);
                activeCanvas = null;
            }
            else
            {
                PauseMenu();
            }
        }
        if (inventoryAction.WasPressedThisFrame())
        {
            if (inventoryCanvas.activeInHierarchy)
            {
                inventoryCanvas.SetActive(false);
                activeCanvas = null;
            }
            else
            {
                InventoryMenu();
            }
        }
    }
    public void PauseMenu()
    {
        pauseCanvas.SetActive(true);
        activeCanvas = pauseCanvas;
    }
    public void InventoryMenu()
    {
        inventoryCanvas.SetActive(true);
        activeCanvas = inventoryCanvas;
    } */
    public void OptionsMenu()
    {
        optionsCanvas.SetActive(true);
        activeCanvas = optionsCanvas;
    }
    public void RestartLevel()
    {
        //button not working removed from ui
        endScreen.SetActive(false);
        SceneManager.LoadScene(1);
    }

}
