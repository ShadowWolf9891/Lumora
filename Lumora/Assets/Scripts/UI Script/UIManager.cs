using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputAction inputAction;
    public static UIManager Instance;
    [SerializeField] GameObject activeCanvas;
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
        inputAction = InputSystem.actions.FindAction("Escape");
    }
    void Update()
    {
        if (inputAction.WasPressedThisFrame())
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
    }
    public GameObject pauseCanvas, optionsCanvas;
    public void OptionsMenu()
    {
        optionsCanvas.SetActive(true);
        activeCanvas = optionsCanvas;
    }
    public void PauseMenu()
    {
        pauseCanvas.SetActive(true);
        activeCanvas = pauseCanvas;
    }
}
