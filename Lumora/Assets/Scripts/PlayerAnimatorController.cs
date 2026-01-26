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
        GameEvents<PlayerInputEvent>.Subscribe(HandleInput);
        GameEvents<UnlockAbilityEvent>.Subscribe(UnlockThrow);
        //GameContext.Instance.OnMove += Move;
        //GameContext.Instance.OnEnterHideState += EnterHide;
        //GameContext.Instance.OnLeaveHideState += LeaveHide;
        //GameContext.Instance.OnThrowReleased += DoThrow;
        //onthrow GameContext.Instance.OnMove += Move;

        animator = GetComponent<Animator>(); 
        behavior = gameObject.GetComponentInParent<PlayerBehavior>();
        rb = behavior.gameObject.GetComponent<Rigidbody>();
        canThrow = false;
    }

	private void HandleInput(PlayerInputEvent e)
	{
        switch (e.ActionType)
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
                if (canThrow) DoThrow();
                break;
            case PlayerInputActionType.Sprint:
                DoSprintToggle();
                break;
            case PlayerInputActionType.Crouch:
                DoCrouchToggle();
                break;
        }
	}

    private void UnlockThrow(UnlockAbilityEvent e) 
    {
        if(e.AbilityName == "throw")
        {
            canThrow = true;
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
    private void DoSprintToggle()
    {
		animator.SetBool("isSprinting", !animator.GetBool("isSprinting"));
    }

    private void DoCrouchToggle()
    {
        animator.SetBool("isHiding", !animator.GetBool("isHiding"));
	}

    public void TriggerSprintNoise()
    {
        behavior.TriggerSprintNoise();
    }
}
