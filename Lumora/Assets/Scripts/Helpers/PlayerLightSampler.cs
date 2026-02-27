using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerLightSampler : MonoBehaviour
{
	[HideInInspector]public float brightness;
	[Header("Light Ref")]
	[SerializeField] Light mainLight;
	[SerializeField] LayerMask shadowMask;

	SphericalHarmonicsL2 sh;

	void Update()
	{
		float indirect = SampleLightProbe();
		float direct = GetDirectionalContribution();
		float local = GetNearbyLights();

		float rawBrightness = indirect + direct + local;

		brightness = Mathf.Clamp01(rawBrightness * 0.1f);
	}

	private float SampleLightProbe()
	{
		Vector3 position = transform.position;

		LightProbes.GetInterpolatedProbe(position, null, out sh);

		// Convert spherical harmonics to approximate luminance
		Color ambient = new Color(
			sh[0, 0],
			sh[1, 0],
			sh[2, 0]
		);

		float luminance =
			0.2126f * ambient.r +
			0.7152f * ambient.g +
			0.0722f * ambient.b;

		return luminance;
	}

	float GetDirectionalContribution()
	{
		if (mainLight == null) return 0f;

		Vector3 dir = -mainLight.transform.forward;

		if (Physics.Raycast(
			transform.position,
			dir,
			out RaycastHit hit,
			100f,
			shadowMask))
		{
			return 0f; // In shadow
		}

		return mainLight.intensity;
	}
	float GetNearbyLights()
	{
		//Can be optimized using a light manager, trigger volumes, and hashing
		float total = 0f;

		foreach (Light l in GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None))
		{
			if (!l.isActiveAndEnabled)
				continue;

			if (l.type == LightType.Directional)
				continue;

			float dist = Vector3.Distance(transform.position, l.transform.position);

			if (dist > l.range)
				continue;

			float attenuation = 1f - (dist / l.range);

			total += l.intensity * attenuation;
		}

		return total;
	}
}