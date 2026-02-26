using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartMenu : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayMusic("TitleTrack");
    }
    public void OnStartClick()
    {
        Debug.Log("Loading scene " + SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
