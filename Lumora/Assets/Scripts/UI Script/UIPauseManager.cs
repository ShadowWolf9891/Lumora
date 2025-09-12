using UnityEngine;

public class UIPauseManager : MonoBehaviour
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
