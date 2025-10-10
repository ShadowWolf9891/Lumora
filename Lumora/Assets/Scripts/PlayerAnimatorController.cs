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
        GameContext.Instance.OnMove += Move;
        GameContext.Instance.OnEnterHideState += EnterHide;
        GameContext.Instance.OnLeaveHideState += LeaveHide;
        GameContext.Instance.OnThrowReleased += DoThrow;
        //onthrow GameContext.Instance.OnMove += Move;

        animator = GetComponent<Animator>(); 
        behavior = gameObject.GetComponentInParent<PlayerBehavior>();
        rb = behavior.gameObject.GetComponent<Rigidbody>();
    }

    private void DoThrow()
    {
        animator.SetTrigger("doThrow");
    }

    private void Update()
    {
        animator.SetFloat("moveSpeed", rb.linearVelocity.normalized.magnitude);
    }

    private void LeaveHide()
    {
        if (animator.GetBool("isHiding"))
        {
            animator.SetBool("isHiding", false);
            animator.SetTrigger("hideStateChanged");
        }
    }

    private void EnterHide()
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
}
