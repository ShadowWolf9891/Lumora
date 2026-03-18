using System;
using UnityEngine;

public enum VisibilityLevels
{
	//note: walking is default level!
	Default,
	Crouching,
	Sprinting,
	Invisible
}

public class VisibilityManager : MonoBehaviour
{
	[SerializeField]
	[Range(0, 1)]
	private float DefaultVisibility = 0.8f; //Starting visibility

    [SerializeField]
    [Range(0, 1)]
    private float SprintingVisibility = 1; //Sprinting visibility

    [SerializeField]
    [Range(0, 1)]
    private float CrouchedVisibility = 0.4f; //Crouching visibility

    [SerializeField]
    [Range(0, 2)]
    private float LightEffectOnVision = 0.5f; //0.5 emphasizes shadow, 2 emphasizes light

    [SerializeField]
    private PlayerLightSampler sampler;

	public float Visibility { get; private set; }
    private PlayerBehavior playerBehavior;
	private void Awake()
	{
		playerBehavior = GetComponent<PlayerBehavior>();
		
        //We're serializing this because we're got getting the component somehow
        //if(!TryGetComponent<LightingSampler>(out sampler))
        //{
        //    Debug.Log("Lighting Sampler not attached to player.");
        //}
        //sampler = GetComponent<LightingSampler>();
	}
	private void Start()
	{
		SetVisibilityLevel(VisibilityLevels.Default);
	}
	private void OnEnable()
	{
		GameEvents<EnterStealthEvent>.Subscribe(EnterStealth);
		GameEvents<LeaveStealthEvent>.Subscribe(ExitStealth);
		GameEvents<PlayerInputEvent>.Subscribe(HandleInputs);
	}
	private void OnDisable()
	{
		GameEvents<EnterStealthEvent>.Unsubscribe(EnterStealth);
		GameEvents<LeaveStealthEvent>.Unsubscribe(ExitStealth);
		GameEvents<PlayerInputEvent>.Unsubscribe(HandleInputs);
	}
	private void SetVisibilityLevel(VisibilityLevels level)
    {
        float lightLevel = Mathf.Pow(sampler.brightness, LightEffectOnVision);
        switch (level)
        {
            case VisibilityLevels.Default:
                Visibility = DefaultVisibility * lightLevel;
                break;
            case VisibilityLevels.Crouching:
                Visibility = CrouchedVisibility * lightLevel;
                break;
            case VisibilityLevels.Sprinting:
                Visibility = SprintingVisibility * lightLevel;
                break;
            case VisibilityLevels.Invisible:
                Visibility = 0f;
                break;
        }
        
    }

    private void HandleInputs(PlayerInputEvent e)
    {
		switch (e.ActionType)
		{
			case PlayerInputActionType.Move:
				if (playerBehavior.IsSprinting)
				{
					SetVisibilityLevel(VisibilityLevels.Sprinting);
				}
                else SetVisibilityLevel(VisibilityLevels.Default);
				break;
		}
    }

    private void EnterStealth(EnterStealthEvent e)
    {
        SetVisibilityLevel(VisibilityLevels.Crouching);
    }
    private void ExitStealth(LeaveStealthEvent e)
    {
        SetVisibilityLevel(VisibilityLevels.Default);
    }

	void OnGUI()
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.fontSize = 20;
		style.normal.textColor = Color.yellow;

		GUI.Label(new Rect(10, 10, 200, 50),
				  "Visibility: " + Visibility.ToString("F2") + $"\nBrightness: {sampler.brightness:F2}", style);
	}
}
