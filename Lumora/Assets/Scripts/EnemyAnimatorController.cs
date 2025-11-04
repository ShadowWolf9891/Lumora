using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    Animator animator;
    EnemyBehavior enemyBehavior;
    Rigidbody rb;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        enemyBehavior = GetComponent<EnemyBehavior>();
    }

    void EnterRunState()
    {

    }
    void EnterPatrolState()
    {

    }
    void EnterIdleState()
    {
        
    }
}
