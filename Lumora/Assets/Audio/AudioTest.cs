using UnityEngine;

public class AudioTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnDistraction1Click()
    {
        AudioManager.Instance.PlaySFX("S_Distraction_1");
    }
    public void OnDistraction2Click()
    {
        AudioManager.Instance.PlaySFX("S_Distraction_2");
    }
    public void OnDistraction3Click()
    {
        AudioManager.Instance.PlaySFX("S_Distraction_3");
    }
}
