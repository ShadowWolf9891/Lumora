using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadZoneTrigger : MonoBehaviour
{
    [SerializeField]
    string[] ScenesToLoad;
    [SerializeField]
    string[] ScenesToUnload;


    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        LoadScenes();
    //        UnloadScenes();
    //    }
    //}

    //private void LoadScenes()
    //{
    //    foreach (string scene in ScenesToLoad)
    //    {
    //        if (!SceneManager.GetSceneByName(scene).isLoaded)
    //        {
    //            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    //            GameEvents<LoadedScene>.Raise(new LoadedScene($"Loaded Scene: {scene}", scene));
    //        }
    //    }
    //}
    //private void UnloadScenes()
    //{
    //    foreach (string scene in ScenesToUnload)
    //    {
    //        if (SceneManager.GetSceneByName(scene).isLoaded)
    //        {
    //            SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.None);
    //            GameEvents<UnloadedScene>.Raise(new UnloadedScene($"Unloaded Scene: {scene}", scene));
    //        }
    //    }
    //}
}
