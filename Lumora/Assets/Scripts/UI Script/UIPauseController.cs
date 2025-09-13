using UnityEngine;

public class UIPauseController : MonoBehaviour
{
    void Start()
    {

    }
    public void ResumeGame()
    {
        this.gameObject.SetActive(false);
    }
    public void OpenOptions()
    {
        this.gameObject.SetActive(false);
        UIManager.Instance.OptionsMenu();
    
    }
}
