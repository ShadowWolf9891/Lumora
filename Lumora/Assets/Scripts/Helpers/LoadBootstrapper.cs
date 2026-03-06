using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadBootstrapper
{
	public static class BootstrapLoader
	{
		private const string BootstrapSceneName = "0_Bootstrap";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void EnsureBootstrapLoaded()
		{
			if (SceneManager.GetSceneByName(BootstrapSceneName).isLoaded)
				return;

			// If we're already in bootstrap, do nothing
			if (SceneManager.GetActiveScene().name == BootstrapSceneName)
				return;

			SceneManager.LoadScene(BootstrapSceneName, LoadSceneMode.Additive);
		}
		
	}
}
