using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartMenu : MonoBehaviour
{
    public void OnStartClick()
    {
        Debug.Log("Loading scene " + SceneManager.GetActiveScene().buildIndex + 1);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
