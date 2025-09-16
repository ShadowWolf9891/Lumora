using UnityEngine;
using EasyBehaviorTree;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    BehaviorTree bt;

    [SerializeField]
    Transform[] patrolPoints;
    int currentPoint = 0;

    NavMeshAgent agent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        bt.Tick(gameObject);
    }
    /// <summary>
    /// Have the enemy patrol between the set points
    /// </summary>
    void Patrol()
    {
        if(agent.remainingDistance < 0.1f)
        {
            if(currentPoint >= patrolPoints.Length - 1) 
            {
                currentPoint = 0;
            }
            else
            {
                currentPoint++;
            }
			agent.SetDestination(patrolPoints[currentPoint].position);
		}

    }

}
