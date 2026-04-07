using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   
    public List<Sound> musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;
    public static AudioManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
	private void OnEnable()
	{
        GameEvents<LoadSceneEvent>.Subscribe(OnLoadScene);
	}
	private void OnDisable()
	{
		GameEvents<LoadSceneEvent>.Unsubscribe(OnLoadScene);
	}

	private void OnLoadScene(LoadSceneEvent e)
	{
        PlayMusic(e.SceneIndex switch
        {
            0 => "TitleTrack",
            1 => "TitleTrack",
            2 => "Chapter1Background",
            3 => "Chapter2Background",
            4 => "TitleTrack",
            _ => throw new NotImplementedException(),
        });
	}

	//checks if current audio source matches the name of the audio source in array
	public void PlayMusic(string name)
    {
        Sound s = musicSounds.Find(x => x.name == name);
        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }
    public void PlaySFX(string name)
    {
        Sound s = sfxSounds.Find(x => x.name == name);
        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    //Toggle audio will mute/unmute audio source based on button input
    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }
    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    //Audio volume is determined by slider value
    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
