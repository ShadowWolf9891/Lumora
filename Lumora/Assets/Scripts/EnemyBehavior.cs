using UnityEngine;
using EasyBehaviorTree;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Accessibility;
using System.Linq;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

public class EnemyBehavior : MonoBehaviour
{
	[SerializeField]
	BehaviorTree btAsset;

    [Header("Vision Properties")]
    [SerializeField]
    float sightRange;
	[SerializeField]
	float alertRange;
	[SerializeField]
    float angleOfVision = 30f;
    [SerializeField]
    float attackRange = 1f;

    [Header("Partol Points")]
    [SerializeField]
    List<Transform> patrolPoints;
    [SerializeField]
    float timeAtEachPatrolPoint = 2f;
    

    [Header("Search Settings")]
	[SerializeField] float searchRadius = 5f;
	[SerializeField] int numberOfSearchPoints = 3;
	[SerializeField] float timeAtEachPoint = 2f;

	[Header("Alerted Properties")]
    [SerializeField]
    float alertedTime = 2f;
   
    NavMeshAgent agent;
    GameObject playerRef;
	BehaviorTree bt;
	BTBlackboard bb;

	EnemyAlertController alertController;
	private float alertedTimer;
	Vector3 lastKnownPlayerLocation;
    AlertStates curState;
    float waitTimer = 0; //Amount of time to wait at each patrol point.
	List<Vector3> searchPoints = new(); //List of points generated when lost sight of player
	int currentPoint = 0;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		agent = GetComponent<NavMeshAgent>();
        playerRef = GameObject.FindGameObjectWithTag("Player");

		
		bt = DeepCloneBehaviorTree(btAsset);
		bb = ScriptableObject.CreateInstance<BTBlackboard>();

		bt.rootNode.SetBlackboard(bb);
		foreach (var node in bt.nodes)
			node.SetBlackboard(bb);

		// Initialize blackboard values
		bb.Set<bool>("CanSeePlayer", false);
		bb.Set<bool>("IsAlerted", false);
		bb.Set<bool>("LostPlayer", false);

		Debug.Log(bb.GetInstanceID());
		alertController = GetComponentInChildren<EnemyAlertController>();
        curState = AlertStates.IDLE;
        OnChangeState();
    }

    // Update is called once per frame
    void Update()
    {
        bt.Tick(gameObject);
        CheckVision();
        //react to other enemies? If a player is spotted by one enemy, should others alert?
    }

    private void CheckVision()
    {
        if(playerRef == null) { return; }

		if (IsObjectInRange(playerRef, sightRange))
		{
			if (CanSeeTarget(playerRef, sightRange))
			{
				if (IsObjectInRange(playerRef, alertRange))
				{
					bb.Set<bool>("CanSeePlayer", true);
				}

				bb.Set<bool>("IsAlerted", true);
				lastKnownPlayerLocation = playerRef.transform.position;
				Debug.Log($"Player in sight range. CanSeePlayer = {bb.Get<bool>("CanSeePlayer")}");
			}
		}
		else if (bb.Get<bool>("IsAlerted"))
		{
			//Search
			bb.Set<bool>("LostPlayer", true);
			lastKnownPlayerLocation = playerRef.transform.position;
			Debug.Log($"Lost Player");
		}
        
    }


    /// <summary>
    /// Have the enemy patrol between the set points
    /// </summary>
    void Patrol()
    {
        if(curState != AlertStates.IDLE) 
        {
            curState = AlertStates.IDLE;
            OnChangeState();
        }

        if (patrolPoints.Count == 0) return; //Return early if no patrol points are specified

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= timeAtEachPatrolPoint)
            {
                currentPoint++;
                waitTimer = 0;

                if (currentPoint >= patrolPoints.Count)
                {
                    currentPoint = 0;
                }
				agent.SetDestination(patrolPoints[currentPoint].position);
			}
           
        }
    }
    void Alerted()
    {
        if (curState != AlertStates.ALERT)
        {
			curState = AlertStates.ALERT;
			OnChangeState();
		}
    }
	void Searching()
	{
		if (searchPoints.Count <= 0) 
        {
            GenerateSearchPoints();
			if (searchPoints.Count > 0)
			{
				agent.SetDestination(searchPoints[0]);
			}
			else
			{
				Debug.Log("No valid search points found — skipping search.");
				EndSearch();
			}
		}
		
		if (agent.remainingDistance <= agent.stoppingDistance)
		{
			waitTimer += Time.deltaTime;

			if (waitTimer >= timeAtEachPoint)
			{
				currentPoint++;

				if (currentPoint < searchPoints.Count)
				{
					agent.SetDestination(searchPoints[currentPoint]);
					waitTimer = 0f;
				}
				else
				{
					EndSearch();
				}
			}
		}
	}

	private void GenerateSearchPoints()
	{
		List<Vector3> points = new List<Vector3>();

		for (int i = 0; i < numberOfSearchPoints; i++)
		{
			Vector2 randomCircle = Random.insideUnitCircle * searchRadius;
			Vector3 candidate = lastKnownPlayerLocation + new Vector3(randomCircle.x, 0f, randomCircle.y);

			if (TryGetNavMeshPoint(candidate, 2f, out Vector3? validPoint))
			{
				points.Add(validPoint.Value);
			}
			else
			{
				Debug.LogWarning($"Could not find NavMesh point near {candidate}");
			}
		}
	}

	private bool TryGetNavMeshPoint(Vector3 candidate, float maxDistance, out Vector3? validPoint)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(candidate, out hit, maxDistance, NavMesh.AllAreas))
		{
			validPoint = hit.position;
			return true;
		}

        validPoint = null;
		return false;
	}

	void Chase()
	{
		if (curState != AlertStates.CHASING)
		{
			curState = AlertStates.CHASING;
			OnChangeState();
		}
        agent.SetDestination(playerRef.transform.position);

		if (IsObjectInRange(playerRef, attackRange))
		{
			Attack();
		}
	}

    private void OnChangeState()
    {
        alertController.ChangeImage(curState);
        currentPoint = 0;
    }
	private void EndSearch()
	{
		bb.Set<bool>("IsAlerted",false);
		bb.Set<bool>("LostPlayer", false);
		bb.Set<bool>("CanSeePlayer", false);
		waitTimer = 0f;
		currentPoint = 0;

		if (patrolPoints.Count > 0)
		{
			agent.SetDestination(patrolPoints[0].position);
		}
	}

	/// <summary>
	/// This function runs when the player is in a set range of an enemy who is alerted.
	/// </summary>
	void Attack()
    {
        //animator.play attack animation
        //agent.SetDestination(transform.position);
        Debug.Log("Tag! you're it");
    }

    /// <summary>
    /// Check if this object can see a target and the target is within a range.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool CanSeeTarget(GameObject target, float range)
    {
        return VisionHelper.CanSeeTarget(gameObject, target, angleOfVision, range, ~0);
    }

    private bool IsObjectInRange(GameObject other, float range)
    {
        return Vector3.Distance(transform.position, other.transform.position) <= range;
    }
	/// <summary>
	/// Fix enemies all using the same scriptable object assets.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	private BehaviorTree DeepCloneBehaviorTree(BehaviorTree source)
	{
		// 1. Clone the top-level BehaviorTree
		BehaviorTree newTree = ScriptableObject.Instantiate(source);

		// 2. Instantiate all nodes
		List<BTNode> newNodes = new List<BTNode>();
		Dictionary<BTNode, BTNode> oldToNew = new Dictionary<BTNode, BTNode>();

		BTNode newRootNode = ScriptableObject.Instantiate(source.rootNode);
		newNodes.Add(newRootNode);
		oldToNew[source.rootNode] = newRootNode;

		foreach (var node in source.nodes)
		{
			BTNode newNode = ScriptableObject.Instantiate(node);
			newNodes.Add(newNode);
			oldToNew[node] = newNode;
		}

		// 3. Fix children and parent links
		foreach (var kvp in oldToNew)
		{
			BTNode oldNode = kvp.Key;
			BTNode newNode = kvp.Value;

			if (oldNode.Children != null && oldNode.Children.Count > 0)
			{
				newNode.Children = new List<BTNode>();
				foreach (var child in oldNode.Children)
				{
					if (oldToNew.TryGetValue(child, out BTNode newChild))
					{
						newNode.Children.Add(newChild);
						newChild.Parent = newNode;
					}
				}
			}
			else
			{
				newNode.Children = new List<BTNode>();
			}
		}

		// 4. Assign the new root node
		newTree.rootNode = oldToNew[source.rootNode];
		newTree.nodes = newNodes;

		return newTree;
	}
}
