using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    //unserialized because i'm lazy and i dont wanna reconnect everything later :/
    Animator animator;
    PlayerBehavior behavior;

    private void Start()
    {
        GameContext.Instance.OnMove += Move;
        GameContext.Instance.OnEnterHideState += EnterHide;
        GameContext.Instance.OnLeaveHideState += LeaveHide;
        //onthrow GameContext.Instance.OnMove += Move;

        animator = GetComponent<Animator>(); 
        //behavior = gameObject.GetComponentInParent<PlayerBehavior>();
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

    private void Move(Vector3 vector)
    {
        //animator.SetFloat("moveSpeed", 1);
    }
    //TODO Add function to set movenment to null

    private void Update()
    {
        
    }
}
