using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadManager()
    {
        GameEvents<LoadSceneEvent>.Subscribe(LoadScene);
    }

	private static void LoadScene(LoadSceneEvent e)
	{
		SceneManager.LoadScene(e.SceneIndex);
	}
}
