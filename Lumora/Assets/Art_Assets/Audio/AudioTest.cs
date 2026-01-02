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
    public void OnFootstep1Click()
    {
        AudioManager.Instance.PlaySFX("S_Footsteps_1");
    }
    public void OnFootstep2Click()
    {
        AudioManager.Instance.PlaySFX("S_Footsteps_2");
    }
    public void OnBodyDropClick()
    {
        AudioManager.Instance.PlaySFX("S_Bodydrop_1");
    }
    public void OnPunchClick()
    {
        AudioManager.Instance.PlaySFX("S_Punch_1");
    }
    public void OnHitGuardClick()
    {
        AudioManager.Instance.PlaySFX("S_HitGuard_1");
    }
}

