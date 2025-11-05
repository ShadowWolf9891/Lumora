using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorController : MonoBehaviour
{
    Animator animator;
    NavMeshAgent agent;
    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();
    }
    public void CheckEnemyState()
    {
        if (agent.desiredVelocity.magnitude > 0.1f) animator.SetBool("IsMoving", true);
        else animator.SetBool("IsMoving", false);
    }
}
