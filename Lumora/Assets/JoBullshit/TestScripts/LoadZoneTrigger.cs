using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadZoneTrigger : MonoBehaviour
{
    [SerializeField]
    SceneField[] ScenesToLoad;
    [SerializeField]
    SceneField[] ScenesToUnload;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadScenes();
            UnloadScenes();
        }
    }

    private void LoadScenes()
    {
        foreach (SceneField scene in ScenesToLoad)
        {
            if (!SceneManager.GetSceneByName(scene.SceneName()).isLoaded)
            {
                SceneManager.LoadSceneAsync(scene.SceneName());
                GameEvents<LoadedScene>.Raise(new LoadedScene($"Loaded Scene: {scene.SceneName()}", scene));
            }
        }
    }
    private void UnloadScenes()
    {
        foreach (SceneField scene in ScenesToUnload)
        {
            if (SceneManager.GetSceneByName(scene.SceneName()).isLoaded)
            {
                SceneManager.UnloadSceneAsync(scene.SceneName());
                GameEvents<UnloadedScene>.Raise(new UnloadedScene($"Unloaded Scene: {scene.SceneName()}", scene));
            }
        }
    }
}
