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

    float savedRunWalkIndex;
    private void Start()
    {
        GameEvents<PlayerInputEvent>.Subscribe(HandleInput);
        GameEvents<EnterStealthEvent>.Subscribe(EnterHide);
        GameEvents<LeaveStealthEvent>.Subscribe(LeaveHide);
        GameEvents<UnlockAbilityEvent>.Subscribe(UnlockThrow);

        animator = GetComponent<Animator>(); 
        behavior = gameObject.GetComponentInParent<PlayerBehavior>();
        rb = behavior.gameObject.GetComponent<Rigidbody>();
        canThrow = false;
    }
    private void OnDestroy()
    {
        GameEvents<PlayerInputEvent>.Unsubscribe(HandleInput);
        GameEvents<EnterStealthEvent>.Unsubscribe(EnterHide);
        GameEvents<LeaveStealthEvent>.Unsubscribe(LeaveHide);
        GameEvents<UnlockAbilityEvent>.Unsubscribe(UnlockThrow);
        GameEvents<ChangeGameStateEvent>.Subscribe(GameStateChanged);
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
        if (rb.linearVelocity.magnitude <= 0.05)
        {
            animator.SetBool("isIdle", true);
            animator.SetFloat("runWalkIndex", 0);
            animator.speed = 1;
        }
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
        if (animator.GetBool("isSprinting"))
        {
            animator.SetFloat("runWalkIndex", 1);
        }
        else
        {
            animator.SetFloat("runWalkIndex", 0);
        }
        if (moveDir.magnitude > 0.05 && animator.GetBool("isIdle")) animator.SetBool("isIdle", false);
        animator.speed = behavior.GetAnimatorSpeedForMovement();
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

    //note: this has to be here because animation event calls 
	public void TriggerSprintNoise()
    {
        behavior.TriggerSprintNoise();
    }

    private void GameStateChanged(ChangeGameStateEvent e)
    {
        if (e.State == GameStates.Running)
        {
            animator.speed = 1;
            if (savedRunWalkIndex != 0)
            {
                animator.SetFloat("runWalkIndex", savedRunWalkIndex);
            }
            savedRunWalkIndex = 0;
        }
        else
        {
            animator.speed = 0;
            savedRunWalkIndex = animator.GetFloat("runWalkIndex");
        }
    }
}
