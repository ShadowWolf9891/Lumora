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

    private void Start()
    {
        GameEvents<PlayerInputEvent>.Subscribe(HandleInput);
        GameEvents<EnterStealthEvent>.Subscribe(EnterHide);
        GameEvents<LeaveStealthEvent>.Subscribe(LeaveHide);
        //GameContext.Instance.OnMove += Move;
        //GameContext.Instance.OnEnterHideState += EnterHide;
        //GameContext.Instance.OnLeaveHideState += LeaveHide;
        //GameContext.Instance.OnThrowReleased += DoThrow;
        //onthrow GameContext.Instance.OnMove += Move;

        animator = GetComponent<Animator>(); 
        behavior = gameObject.GetComponentInParent<PlayerBehavior>();
        rb = behavior.gameObject.GetComponent<Rigidbody>();
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
                DoThrow();
				break;
            case PlayerInputActionType.Sprint:
                DoSprintToggle();
                break;
		}
	}


    private void Update()
    {
        animator.SetFloat("moveSpeed", rb.linearVelocity.normalized.magnitude);
    }

    private void LeaveHide(LeaveStealthEvent e)
    {
        if (animator.GetBool("isHiding"))
        {
            animator.SetBool("isHiding", false);
            animator.SetTrigger("hideStateChanged");
        }
    }

    private void EnterHide(EnterStealthEvent e)
    {
        if (!animator.GetBool("isHiding"))
        {
            animator.SetBool("isHiding", true);
            animator.SetTrigger("hideStateChanged");
        }
    }

    private void Move(Vector3 moveDir)
    {
        animator.SetTrigger("doMovement");
        animator.SetFloat("moveSpeed", moveDir.normalized.magnitude);
    }
    private void DoThrow()
    {
        animator.SetTrigger("doThrow");
    }
    private void DoSprintToggle()
    {
        Debug.Log("Did sprint toggle animator");
        if (animator.GetBool("isSprinting")) { animator.SetBool("isSprinting", false); }
        else { animator.SetBool("isSprinting", true); }
    }

    public void TriggerSprintNoise()
    {
        behavior.TriggerSprintNoise();
    }
}
