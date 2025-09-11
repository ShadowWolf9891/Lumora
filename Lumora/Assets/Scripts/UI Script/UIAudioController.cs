using UnityEngine;
using UnityEngine.UI;

public class UIAudioController : MonoBehaviour
{
    //UI interactions with Audio Manager
    //Attached to options canvas so will only be active when in canvas
    public Slider musicSlider, sfxSlider;
    void Awake()
    {

    }

    //button press to mute/unmute audio
    public void ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
    }
    public void ToggleSFX()
    {
        AudioManager.Instance.ToggleSFX();
    }

    //Sets audio volume to value from slider
    public void MusicVolume()
    {
        AudioManager.Instance.MusicVolume(musicSlider.value);
    }
    public void SFXVolume()
    {
        AudioManager.Instance.SFXVolume(sfxSlider.value);
    }
}