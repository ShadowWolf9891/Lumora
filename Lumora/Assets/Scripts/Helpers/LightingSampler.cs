using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Class to sample the 
/// </summary>
public class LightingSampler : MonoBehaviour
{
    public Camera probeCam; //Camera child of Player to detect lighting
	public int textureSize = 16; //Smaller = faster, less accurate
	public float brightness = 0f;      // Last known brightness (0-1)
	public float sampleInterval = 0.3f; // seconds between checks

	private RenderTexture rt;
	private bool requestPending = false;

	private void Start()
	{
		rt = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
		rt.Create();
		probeCam.targetTexture = rt;

		// Start periodic sampling coroutine
		StartCoroutine(SampleRoutine());
	}

	/// <summary>
	/// Sample the lighting at a fixed interval.
	/// </summary>
	/// <returns></returns>
	IEnumerator SampleRoutine()
	{
		while (true)
		{
			if (!requestPending)
			{
				probeCam.Render();
				AsyncGPUReadback.Request(rt, 0, TextureFormat.RGB24, OnCompleteReadback);
				requestPending = true;
			}
			yield return new WaitForSeconds(sampleInterval);
		}
	}

	void OnCompleteReadback(AsyncGPUReadbackRequest req)
	{
		requestPending = false;

		if (req.hasError)
		{
			Debug.LogWarning("GPU readback error.");
			return;
		}

		// Get raw pixel data
		var data = req.GetData<Color32>();

		long sum = 0;
		for (int i = 0; i < data.Length; i++)
		{
			// Convert to grayscale manually (fast integer math)
			Color32 c = data[i];
			sum += (c.r + c.g + c.b);
		}

		float avg = (sum / (float)(data.Length * 3 * 255f));
		brightness = avg;
	}
}
