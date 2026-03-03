using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    //unserialized because i'm lazy and i dont wanna reconnect everything later :/
    Animator animator;
    PlayerBehavior behavior;
    Rigidbody rb;
    bool canThrow = false;

    private void Start()
    {
        GameEvents<ChangeGameStateEvent>.Subscribe(GameStateChange);
        GameEvents<PlayerInputEvent>.Subscribe(HandleInput);
        GameEvents<EnterStealthEvent>.Subscribe(EnterHide);
        GameEvents<LeaveStealthEvent>.Subscribe(LeaveHide);
        GameEvents<UnlockAbilityEvent>.Subscribe(UnlockThrow);

        animator = GetComponent<Animator>(); 
        behavior = gameObject.GetComponentInParent<PlayerBehavior>();
        rb = behavior.gameObject.GetComponent<Rigidbody>();
        canThrow = false;
    }

    private void GameStateChange(ChangeGameStateEvent e)
    {
        if (e.State == GameStates.Running)
        {
            //unpause
            animator.speed = 1;
        }
        else
        {
            //this pauses things
            animator.speed = 0;
        }
    }

    private void HandleInput(PlayerInputEvent e)
	{
		switch(e.ActionType) 
        {
            case PlayerInputActionType.Move:
                Move(e.MoveDirection);
                break;
			case PlayerInputActionType.Jump:
                //Jump animation
				break;
			case PlayerInputActionType.Throw:
                //Prepare throw animation?
				break;
			case PlayerInputActionType.ThrowRelease:
                if(canThrow) DoThrow();
				break;
            case PlayerInputActionType.Sprint:
                DoSprintToggle();
                break;
            case PlayerInputActionType.Crouch:
				DoCrouchToggle();
                break;
		}
	}

	private void Update()
    {
        animator.SetFloat("moveSpeed", rb.linearVelocity.normalized.magnitude);
    }

    private void UnlockThrow(UnlockAbilityEvent e) 
    {
        if(e.AbilityName == "throw")
        {
            canThrow = true;
        }
    }
	private void EnterHide(EnterStealthEvent e)
	{
		if (!animator.GetBool("isHiding"))
		{
			animator.SetBool("isHiding", true);
		}
	}
	private void LeaveHide(LeaveStealthEvent e)
    {
        if (animator.GetBool("isHiding"))
        {
            animator.SetBool("isHiding", false);
        }
    }
    private void Move(Vector3 moveDir)
    {
        animator.SetFloat("moveSpeed", moveDir.magnitude);
    }
    private void DoThrow()
    {
        animator.SetTrigger("doThrow");
    }
	private void DoCrouchToggle()
	{
        animator.SetBool("isCrouched", !animator.GetBool("isCrouched"));
	}
	private void DoSprintToggle()
	{
		animator.SetBool("isSprinting", !animator.GetBool("isSprinting"));
	}

	public void TriggerSprintNoise()
    {
        behavior.TriggerSprintNoise();
    }
}
