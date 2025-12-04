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
        enemyBehavior = GetComponentInParent<EnemyBehavior>();
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
        animator.SetBool("IsAttacking", true);
    }
    public void OnEnemyAttackEvent()     //Triggers from animation event. Reduces player health via event.
    {
        AudioManager.Instance.PlaySFX("S_Punch_1");
        GameEvents<PlayerDamagedEvent>.Raise(new PlayerDamagedEvent("Player Damaged Event", enemyBehavior.attackDamage));
        Invoke("UnlockEnemyMovement", enemyBehavior.attackLockoutTime);
    }

    private void UnlockEnemyMovement()
    {
        animator.SetBool("IsAttacking", false);
        enemyBehavior.EndAttackState();
    }
}
