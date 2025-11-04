using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void CheckEnemyState(AlertStates alertState)
    {
        if (animator == null) Debug.Log("BAD");
        if (alertState == AlertStates.IDLE)
        {
            animator.SetBool("IsAlerted", false);
            animator.SetBool("IsPartol", false);
            if (!animator.GetBool("IsIdle")) animator.SetBool("IsIdle", true);
        }
        if (alertState == AlertStates.ALERT)
        {
            animator.SetBool("IsAlerted", false);
            animator.SetBool("IsIdle", false);
            if (!animator.GetBool("IsPatrol")) animator.SetBool("IsPatrol", true);
        }
        if(alertState == AlertStates.CHASING)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsPartol", false);
            if (!animator.GetBool("IsAlerted")) animator.SetBool("IsAlerted", true);
        }
    }
}
