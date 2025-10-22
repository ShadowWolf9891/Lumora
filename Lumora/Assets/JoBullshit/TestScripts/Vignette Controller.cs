using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Splines.Interpolators;

public class VignetteController : MonoBehaviour
{
    [Header("Volume Values")]
    [SerializeField]
    float minValue;
    [SerializeField]
    float maxValue;

    private bool vignetteActive;
    private Volume volume;

    private void Start()
    {
        volume = GetComponent<Volume>();
        GameEvents<EnterStealthEvent>.Subscribe(EnterHiding);
		GameEvents<LeaveStealthEvent>.Subscribe(LeaveHiding);
		//GameContext.Instance.OnEnterHideState += EnterHiding;
        //GameContext.Instance.OnLeaveHideState += LeaveHiding;
    }
    private void EnterHiding(EnterStealthEvent e)
    {
        if (!vignetteActive)
        {
            Debug.Log("vignette activated");
            vignetteActive = true;
            volume.weight = Mathf.Lerp(minValue, maxValue, 3);
        }
    }
    private void LeaveHiding(LeaveStealthEvent e)
    {
        if (vignetteActive)
        {
            Debug.Log("vignette Deactivated");
            vignetteActive = false;
            volume.weight = Mathf.Lerp(maxValue, minValue, 3);
        }
    }
}
