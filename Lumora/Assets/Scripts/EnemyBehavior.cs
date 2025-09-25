using UnityEngine;
using EasyBehaviorTree;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    BehaviorTree bt;
    BTBlackboard bb;

    [SerializeField]
    Transform[] patrolPoints;
    int currentPoint = 0;

    [SerializeField]
    float sightRange;
    [SerializeField]
    float angleOfVision = 30f;
    [SerializeField]
    float attackRange = 1f;


    NavMeshAgent agent;
    GameObject playerRef;
    Transform eyesTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        bb = bt.rootNode.GetBlackboard();
        eyesTransform = this.gameObject.transform.GetChild(1);
        playerRef = GameObject.Find("Player");
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

        Vector3 dirToPlayer = -(eyesTransform.position - playerRef.gameObject.transform.position);
        float raycastAngle = Vector3.Angle(eyesTransform.position, playerRef.gameObject.transform.position);
        if (raycastAngle < angleOfVision)
        {
            if (Physics.Raycast(eyesTransform.position, dirToPlayer, sightRange, LayerMask.GetMask("Player")))
            {
                Debug.Log("spotted!");
                bb.Set<bool>("CanSeePlayer", true);
            }
        }


    }

    /// <summary>
    /// Have the enemy chase the player. 
    /// </summary>
    void Alert()
    {
        agent.SetDestination(playerRef.transform.position);

        if (agent.remainingDistance < 1)
        {
            Attack();
            bb.Set<bool>("CanSeePlayer", false);
        }
    }

    /// <summary>
    /// This function runs when the player is in a set range of an enemy who is alerted.
    /// </summary>
    void Attack()
    {
        Debug.Log("Tag! you're it");
    }

}
