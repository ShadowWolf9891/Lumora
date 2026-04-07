using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu Start sequence.
/// References are assigned at runtime by UIManager.
/// Attach this to the root of the MainMenuElement prefab.
/// </summary>
public class MainMenuTransition : MonoBehaviour
{
	[HideInInspector] public CanvasGroup illustration1;
	[HideInInspector] public CanvasGroup illustration2;
	[HideInInspector] public CanvasGroup lumoraTitle1;
	[HideInInspector] public CanvasGroup lumoraTitle2;
	[HideInInspector] public CanvasGroup buttonsGroup;
	[HideInInspector] public CanvasGroup blackOverlay;

	[Header("Timing")]
	public float buttonsFadeDuration = 0.4f;
	public float crossfadeDuration = 1.2f;
	public float holdOnImage2Duration = 2.0f;

	private bool _isTransitioning = false;

	private void OnEnable()
	{
		Load();
	}
	void Load()
	{
		// Reset to initial state every time the main menu is shown
		if (illustration1 != null) illustration1.alpha = 1f;
		if (illustration2 != null) illustration2.alpha = 0f;
		if (lumoraTitle1 != null) lumoraTitle1.alpha = 1f;
		if (lumoraTitle2 != null) lumoraTitle2.alpha = 0f;
		if (buttonsGroup != null)
		{
			buttonsGroup.alpha = 1f;
			buttonsGroup.interactable = true;
			buttonsGroup.blocksRaycasts = true;
		}
		if (blackOverlay != null) blackOverlay.alpha = 0f;
		_isTransitioning = false;
	}

	public void OnStartButtonPressed()
	{
		if (_isTransitioning) return;
		if (illustration1 == null) { Debug.LogError("[MainMenuTransition] illustration1 is null"); return; }
		if (illustration2 == null) { Debug.LogError("[MainMenuTransition] illustration2 is null"); return; }

		_isTransitioning = true;
		StartCoroutine(TransitionSequence());
	}

	private IEnumerator TransitionSequence()
	{
		// 1. Fade out buttons
		yield return StartCoroutine(FadeCanvasGroup(buttonsGroup, 1f, 0f, buttonsFadeDuration));
		if (buttonsGroup != null)
		{
			buttonsGroup.interactable = false;
			buttonsGroup.blocksRaycasts = false;
		}

		// 2. Crossfade illustrations + titles
		yield return StartCoroutine(CrossfadeIllustrations());

		// 3. Hold on Illustration 2
		yield return new WaitForSeconds(holdOnImage2Duration);

		// 4. Load next scene
		EventManager.Instance.Raise(new LoadSceneEvent("StartNewGame", SceneManager.GetActiveScene().buildIndex + 2));
	}

	private IEnumerator CrossfadeIllustrations()
	{
		float elapsed = 0f;

		while (elapsed < crossfadeDuration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / crossfadeDuration);

			if (illustration1 != null) illustration1.alpha = 1f - t;
			if (illustration2 != null) illustration2.alpha = t;
			if (lumoraTitle1 != null) lumoraTitle1.alpha = 1f - t;
			if (lumoraTitle2 != null) lumoraTitle2.alpha = t;

			yield return null;
		}

		if (illustration1 != null) illustration1.alpha = 0f;
		if (illustration2 != null) illustration2.alpha = 1f;
		if (lumoraTitle1 != null) lumoraTitle1.alpha = 0f;
		if (lumoraTitle2 != null) lumoraTitle2.alpha = 1f;
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
	{
		if (cg == null) yield break;

		float elapsed = 0f;
		cg.alpha = from;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
			yield return null;
		}

		cg.alpha = to;
	}
}