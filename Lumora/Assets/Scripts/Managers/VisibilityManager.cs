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

	public float Visibility { get; private set; }
    private PlayerBehavior playerBehavior;
	private void Start()
	{
		playerBehavior = GetComponent<PlayerBehavior>();
		GameEvents<EnterStealthEvent>.Subscribe(EnterStealth);
		GameEvents<LeaveStealthEvent>.Subscribe(ExitStealth);
		GameEvents<PlayerInputEvent>.Subscribe(HandleInputs);
	}

    private void SetVisibilityLevel(VisibilityLevels level)
    {
        switch (level)
        {
            case VisibilityLevels.Default:
                Visibility = DefaultVisibility;
                break;
            case VisibilityLevels.Crouching:
                Visibility = CrouchedVisibility;
                break;
            case VisibilityLevels.Sprinting:
                Visibility = SprintingVisibility;
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
				if (playerBehavior.isSprinting)
				{
					SetVisibilityLevel(VisibilityLevels.Sprinting);
				}
                SetVisibilityLevel(VisibilityLevels.Default);
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
}
