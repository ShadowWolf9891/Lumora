using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartMenu : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayMusic("TitleTrack");
    }
    
}
