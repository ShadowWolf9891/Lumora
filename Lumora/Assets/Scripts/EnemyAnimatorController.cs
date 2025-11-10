using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorController : MonoBehaviour
{
    EnemyBehavior enemyBehavior;
    Animator animator;
    NavMeshAgent agent;
    void Awake()
    {
        enemyBehavior = GetComponent<EnemyBehavior>();
        animator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();
    }
    public void CheckEnemyState()
    {
        if (agent.desiredVelocity.magnitude > 0.1f) animator.SetBool("IsMoving", true);
        else animator.SetBool("IsMoving", false);
    }
    public void DoAttack()
    {
        animator.SetTrigger("DoAttack");
    }
    public void OnEnemyAttackEvent()     //Triggers from animation event. Reduces player health via event.
    {
        GameEvents<PlayerDamagedEvent>.Raise(new PlayerDamagedEvent("Player Damaged Event", enemyBehavior.attackDamage));
    }
}
