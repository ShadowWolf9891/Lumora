using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
	[Range(0, 1)]
	public float VisibilityLevel { get; private set; } = 0.8f; //Starting visibility


	private void Start()
	{
		GameContext.Instance.OnEnterHideState += EnterStealth;
		GameContext.Instance.OnLeaveHideState += ExitStealth;
	}

	/// <summary>
	/// Amount to increase the visibility by. Clamped between 0 and 1.
	/// </summary>
	/// <param name="amount"></param>
	public void IncreaseVisibility(float amount)
	{
		if (VisibilityLevel + amount <= 1 && VisibilityLevel + amount >= 0) 
		{
			VisibilityLevel += amount;
		}
	}
	/// <summary>
	/// Amount to decrease the visibility by. Clamped between 0 and 1. For example, entering stealth might have amount = 0.5;
	/// </summary>
	/// <param name="amount"></param>
	public void DecreaseVisibility(float amount)
	{
		if (VisibilityLevel - amount <= 1 && VisibilityLevel - amount >= 0)
		{
			VisibilityLevel -= amount;
		}
	}

	private void EnterStealth()
	{
		DecreaseVisibility(0.4f);
	}
	private void ExitStealth()
	{
		IncreaseVisibility(0.4f);
	}
	//Other stuff that may increase or decrease visibility that is consistent

}
