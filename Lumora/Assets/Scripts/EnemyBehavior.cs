using UnityEngine;
using EasyBehaviorTree;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Behavior Tree")]
    [SerializeField]
    BehaviorTree bt;
    BTBlackboard bb;

    [Header("Vision Properties")]
    [SerializeField]
    float sightRange;
    [SerializeField]
    float angleOfVision = 30f;
    [SerializeField]
    float attackRange = 1f;

    [Header("Partol Points")]
    [SerializeField]
    Transform[] patrolPoints;
    int currentPoint = 0;

    [Header("Alerted Properties")]
    [SerializeField]
    float alertedTime = 0;
    private float alertedTimer;

    NavMeshAgent agent;
    GameObject playerRef;
    Transform eyesTransform;
    EnemyAlertController alertController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        bb = bt.rootNode.GetBlackboard();
        bb.Set<bool>("CanSeePlayer", false);
        bb.Set<bool>("IsAlerted", false);
        eyesTransform = this.gameObject.transform.GetChild(1);
        playerRef = GameObject.FindGameObjectWithTag("Player");
        //theres gotta be a better way to do this but we ball
        alertController = gameObject.transform.GetChild(2).GetChild(0).GetComponent<EnemyAlertController>();
    }

    // Update is called once per frame
    void Update()
    {
        bt.Tick(gameObject);
        //react to other enemies? If a player is spotted by one enemy, should others alert?
    }
    /// <summary>
    /// Have the enemy patrol between the set points
    /// </summary>
    void Patrol()
    {
        //0 is patrol
        alertController.ChangeImage(0);
        if (agent.remainingDistance < 0.1f)
        {
            if (currentPoint >= patrolPoints.Length - 1)
            {
                currentPoint = 0;
            }
            else
            {
                currentPoint++;
            }
            agent.SetDestination(patrolPoints[currentPoint].position);
        }

        if (CanSeePlayer())
        {
            SpotPlayer();
        }
    }

    private void SpotPlayer()
    {
        alertedTimer = alertedTime;
        agent.isStopped = false;
        //Debug.Log("Enemy can see player");
        if (bb.Get<bool>("IsAlerted") == false)
        {
            bb.Set<bool>("IsAlerted", true);
            GameContext.Instance.RaisePlayerSpotted();
        }
        if (bb.Get<bool>("CanSeePlayer") == false)
        {
            bb.Set<bool>("CanSeePlayer", true);
            GameContext.Instance.RaisePlayerSpotted();
        }
    }

    private bool CanSeePlayer()
    {
        
        Vector3 dirToPlayer = -(eyesTransform.position - playerRef.gameObject.transform.position);
        float raycastAngle = Vector3.Angle(eyesTransform.position, playerRef.gameObject.transform.position);
        if (raycastAngle < angleOfVision)
        {
            RaycastHit visionHit;
            if (Physics.Raycast(eyesTransform.position, dirToPlayer, out visionHit, sightRange, ~0))
            {
                if (visionHit.collider.gameObject.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Have the enemy chase the player. 
    /// </summary>
    void Chase()
    {
        //2 is for chase
        alertController.ChangeImage(2);
        agent.SetDestination(playerRef.transform.position);

        if (agent.remainingDistance < attackRange)
        {
            Attack();
            bb.Set<bool>("CanSeePlayer", false);
        }

        if (!CanSeePlayer())
        {
            bb.Set<bool>("CanSeePlayer", false);
        }
    }

    void Searching()
    {
        //1 is Alert
        alertController.ChangeImage(1);
        //agent isn't going to go to the exact point, i'm using attackrange as a placeholder "close enough" point
        if (agent.remainingDistance <= attackRange)
        {
            agent.isStopped = true;
            //currentley we're just rotating transform. I'd like to play an animation where we can do exactly this
            // we could also do a baldurs gate 3 thing where we just rotate the character back and forth lol
            transform.Rotate(new Vector3(0, 3, 0));
        }

        if (CanSeePlayer())
        {
            SpotPlayer();
            //agent.isStopped = false;
        }

        if (alertedTimer >= 0)
        {
            alertedTimer -= Time.deltaTime;
        }
        else
        {
            agent.isStopped = false;
            bb.Set<bool>("IsAlerted", false);
        }
    }

    /// <summary>
    /// This function runs when the player is in a set range of an enemy who is alerted.
    /// </summary>
    void Attack()
    {
        //animator.play attack animation
        agent.SetDestination(transform.position);
        Debug.Log("Tag! you're it");
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.white;
    //    Gizmos.DrawWireSphere(transform.position, sightRange);
    //    Gizmos.color = Color.darkRed;
    //    Gizmos.DrawLine(eyesTransform.position, new Vector3(eyesTransform.position.x, eyesTransform.position.y, eyesTransform.position.z + sightRange));
    //}
}
