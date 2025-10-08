using System;
using UnityEngine;

public class GameContext
{
	private static GameContext _instance;
	public static GameContext Instance => _instance ??= new GameContext();

	//Events that can be called from anywhere

	//Player controls
	public event Action<Vector3> OnMove;
	public event Action OnAttackPressed;
	public event Action OnInteractPressed;
	public event Action OnHidePressed;
	public event Action OnJumpPressed;
	public event Action OnThrowPressed;

    //Player Event Context
	public event Action OnEnterHideState;
	public event Action OnLeaveHideState;
	public event Action OnPlayerSpotted;

	//Player Controls
	public void RaiseMove(Vector3 move) {  OnMove?.Invoke(move); }
	public void RaiseAttack() { OnAttackPressed?.Invoke(); }
	public void RaiseInteract() {  OnInteractPressed?.Invoke(); }
	public void RaiseHidePressed() {  OnHidePressed?.Invoke(); }
	public void RaiseJumpPressed() {  OnJumpPressed?.Invoke(); }
	public void RaiseThrowPressed() { OnThrowPressed?.Invoke(); }

	//Player Events
	public void RaiseEnterStealth() { OnEnterHideState?.Invoke(); }
    public void RaiseLeaveStealth() { OnLeaveHideState?.Invoke(); }
    public void RaisePlayerSpotted() { OnPlayerSpotted?.Invoke(); }
}
