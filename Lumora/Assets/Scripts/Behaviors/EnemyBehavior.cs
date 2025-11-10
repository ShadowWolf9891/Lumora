using UnityEngine;
using EasyBehaviorTree;
using UnityEngine.AI;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem.XR;

public class EnemyBehavior : MonoBehaviour
{
	#region Properties
	[SerializeField]
	BehaviorTree btAsset;

	[Header("Vision Properties")]
	[SerializeField]
	float chasingRange;
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
	[SerializeField] float searchRadius = 10f;
	[SerializeField] int numberOfSearchPoints = 3;
	[SerializeField] float timeAtEachPoint = 1f;

	[Header("Alerted Properties")]
	[SerializeField]
	float alertedTime = 2f;

	[Header("Attack Damage")]
	[SerializeField]
	[Range(1, 10)]
	public int attackDamage = 4;

    NavMeshAgent agent;
	GameObject playerRef;
	BehaviorTree bt;
	BTBlackboard bb;

	EnemyAlertController alertController;
	Vector3 lastKnownPlayerLocation;
	AlertStates curState;
	float waitTimer = 0; //Amount of time to wait at each patrol point.
	List<Vector3> searchPoints = new(); //List of points generated when lost sight of player
	int curPatrolPoint = 0;
	Vector3 previousVelocity;

	string bb_CanSeePlayer = "CanSeePlayer";
	string bb_IsAlerted = "IsAlerted";
	string bb_LostPlayer = "LostPlayer";

    //[SerializeField] GameObject endScreen;
	EnemyAnimatorController animController;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		playerRef = GameObject.FindGameObjectWithTag("Player");
		animController = GetComponentInChildren<EnemyAnimatorController>();

		bb = ScriptableObject.CreateInstance<BTBlackboard>();
		bt = DeepCloneBehaviorTree(btAsset);
		
		// Initialize blackboard values
		bb.Set<bool>(bb_CanSeePlayer, false);
		bb.Set<bool>(bb_IsAlerted, false);
		bb.Set<bool>(bb_LostPlayer, false);

		alertController = GetComponentInChildren<EnemyAlertController>();
		curState = AlertStates.IDLE;
		OnChangeState();

		GameEvents<ChangeGameStateEvent>.Subscribe(FreezeEnemy);
		//GameContext.Instance.OnPauseGame += FreezeEnemy;
		//GameContext.Instance.OnUnPauseGame += UnFreezeEnemy;
	}

	// Update is called once per frame
	void Update()
	{
		CheckVision();
		bt.Tick(gameObject);
		animController.CheckEnemyState();
	}

	#region Vision
	private void CheckVision()
	{
		if (playerRef == null) return;

		float distance = Vector3.Distance(transform.position, playerRef.transform.position);
		bool canSee = false;

		// Only do expensive raycast if within alert range
		if (distance <= alertRange)
		{
			canSee = CanSeeTarget(playerRef, alertRange);
			if (canSee && !bb.Get<bool>(bb_IsAlerted))
				bb.Set<bool>(bb_IsAlerted, true);
		}

		if (distance <= chasingRange && canSee)
		{
			if (!bb.Get<bool>(bb_CanSeePlayer)) bb.Set<bool>(bb_CanSeePlayer, true);
			if (!bb.Get<bool>(bb_LostPlayer)) bb.Set<bool>(bb_LostPlayer, true);
		}
		else
		{
			if (bb.Get<bool>(bb_CanSeePlayer))
			{
				bb.Set<bool>(bb_CanSeePlayer, false);
			}
				
		}
	}
	#endregion
	//States are controlled by the behavior tree
	#region States
	/// <summary>
	/// Have the enemy patrol between the set points
	/// </summary>
	public void Patrol()
	{
		if (curState != AlertStates.IDLE)
		{
			curState = AlertStates.IDLE;
			OnChangeState();
		}

		if (patrolPoints.Count == 0) return;

		if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
		{
			waitTimer += Time.deltaTime;
			if (waitTimer >= timeAtEachPatrolPoint)
			{
				curPatrolPoint = (curPatrolPoint + 1) % patrolPoints.Count;
				agent.SetDestination(patrolPoints[curPatrolPoint].position);
				waitTimer = 0f;
			}
		}
	}

	public void Alerted()
	{
		if (curState != AlertStates.ALERT)
		{
			curState = AlertStates.ALERT;
			OnChangeState();
		}

		waitTimer += Time.deltaTime;
		if (waitTimer >= alertedTime) //Wait an amount of time alerted
		{
			waitTimer = 0f;
			if (patrolPoints.Count > 0)
			{
				agent.SetDestination(GetClosestPoint(transform.position, patrolPoints)); //Go back to patrol path
			}

			bb.Set<bool>(bb_IsAlerted, false); //Stop being alerted
		}
	}

	public void Searching()
	{
		if (searchPoints.Count <= 0 && bb.Get<bool>(bb_LostPlayer)) 
		{
			curState = AlertStates.ALERT;
			GameEvents<EnemyDropsAlert>.Raise(new EnemyDropsAlert($"EnemyDropsAlert: {gameObject.name}", this.gameObject));
			OnChangeState();
			GenerateSearchPoints();
			if(searchPoints.Count <= 0)
			{
				bb.Set<bool>(bb_LostPlayer, false);
				Debug.Log("No valid search points");
				return;
			}
			agent.SetDestination(GetClosestPoint(lastKnownPlayerLocation, searchPoints));
			waitTimer = timeAtEachPoint;
		}

		if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
		{
			waitTimer += Time.deltaTime;
			if (waitTimer >= timeAtEachPoint)
			{
				waitTimer = 0f;
				searchPoints.Remove(GetClosestPoint(transform.position, searchPoints));
				if (searchPoints.Count > 0)
				{
					agent.SetDestination(GetClosestPoint(lastKnownPlayerLocation, searchPoints));
				}
				else
				{
					bb.Set<bool>(bb_LostPlayer, false);
				}
			}
		}
	}

	public void Chase()
	{
		if (playerRef == null) return;

		if (curState != AlertStates.CHASING)
		{
			curState = AlertStates.CHASING;
			GameEvents<PlayerSpottedEvent>.Raise(new PlayerSpottedEvent("PlayerSpottedEvent", this.gameObject));
			OnChangeState();
		}

		Vector3 predictedPos = playerRef.transform.position + playerRef.GetComponent<Rigidbody>().linearVelocity * 0.5f;

		agent.SetDestination(predictedPos);

		lastKnownPlayerLocation = predictedPos;

		if (IsObjectInRange(playerRef, attackRange))
		{
			Attack();
		}
	}

	private void Attack()
	{
		animController.DoAttack();
		agent.destination = transform.position;
		//TODO: lock out movement for a second
	}
	#endregion

	private void OnChangeState()
	{
		alertController.ChangeImage(curState);
		searchPoints.Clear();
	}

	private void FreezeEnemy(ChangeGameStateEvent e) 
	{
		if(e.State == GameStates.Running)
		{
			agent.velocity = previousVelocity;
			agent.isStopped = false;
		}
		else if (e.State == GameStates.Paused || e.State == GameStates.Dialogue || e.State == GameStates.Game_Over) //Not sure what to do with cutscenes yet
		{
			previousVelocity = agent.velocity;
			agent.velocity = Vector3.zero;
			agent.isStopped = true;
		}

	}

	/// <summary>
	/// Check if this object can see a target and the target is within a range.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="range"></param>
	/// <returns></returns>
	private bool CanSeeTarget(GameObject target, float range)
	{
		return VisionHelper.CanSeeTarget(gameObject, target, angleOfVision, range, LayerMask.GetMask("Default", "Player"));
	}
	private bool IsObjectInRange(GameObject other, float range)
	{
		return Vector3.Distance(transform.position, other.transform.position) <= range;
	}
	private void GenerateSearchPoints()
	{
		searchPoints.Clear();
		if (NavMesh.SamplePosition(lastKnownPlayerLocation, out NavMeshHit hit, 5.0f, agent.areaMask))
		{
			searchPoints.Add(hit.position);
		}
		else
		{
			Debug.LogWarning($"Could not find NavMesh point near {lastKnownPlayerLocation}. Player is likely off the navmesh");
			return;
		}
		for (int i = 1; i < numberOfSearchPoints; i++)
		{
			if (GetRandomPoint(searchPoints[i-1], searchRadius, out Vector3 validPoint))
			{
				searchPoints.Add(validPoint);
			}
			else
			{
				Debug.LogWarning($"Could not find NavMesh point near {lastKnownPlayerLocation}");
				searchPoints.Add(lastKnownPlayerLocation);
			}
		}
	}
	bool GetRandomPoint(Vector3 center, float range, out Vector3 result)
	{
		for (int i = 0; i < 30; i++)
		{
			float angle = Random.Range(-angleOfVision, angleOfVision);
			float distance = Random.Range(2f, range);
			Quaternion rotation = Quaternion.Euler(0, angle, 0);
			Vector3 direction = rotation * transform.forward;
			Vector3 randomPoint = center + direction * distance;

			if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, agent.areaMask))
			{
				result = hit.position;
				return true;
			}
		}
		
		result = Vector3.zero;
		return false;
	}

	/// <summary>
	/// Gets the closest point of a list of transforms
	/// </summary>
	/// <param name="startPos"></param>
	/// <param name="points"></param>
	/// <returns></returns>
	private Vector3 GetClosestPoint(Vector3 startPos, List<Transform> points)
	{
		List<Vector3> v3Points = new List<Vector3>();
		foreach(var point in points)
		{
			v3Points.Add(point.position);
		}
		return GetClosestPoint(startPos, v3Points);

	}
	/// <summary>
	/// Gets the closest point of a list of vector3's
	/// </summary>
	/// <param name="startPos"></param>
	/// <param name="points"></param>
	/// <returns></returns>
	private Vector3 GetClosestPoint(Vector3 startPos, List<Vector3> points)
	{
		if (searchPoints.Count <= 0)
		{
			return startPos;
		}
		//Get closest point to start search at.
		Vector3 closestPoint = points[0];
		float closestDistance = Vector3.Distance(points[0], transform.position);

		foreach (Vector3 point in points)
		{
			float dist = Vector3.Distance(point, transform.position);
			if (dist < closestDistance)
			{
				closestDistance = dist;
				closestPoint = point;
			}
		}
		return closestPoint;
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
		oldToNew[source.rootNode] = newRootNode;
		
		foreach (var node in source.nodes)
		{
			if (node == source.rootNode) continue;
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

		foreach (var node in newNodes)
		{
			node.SetBlackboard(bb);
		}
		newTree.rootNode.SetBlackboard(bb);
		// 4. Assign the new root node
		newTree.rootNode = oldToNew[source.rootNode];
		newTree.nodes = newNodes;

		return newTree;
	}

	/// <summary>
	/// To be called from NoiseBehavior upon collision with a noise ping. 
	/// Dependent on ping info, causes enemy to alert and investigate, or chase the player.
	/// </summary>
	public void OnHearNoise(Vector3 noiseLocation, bool isPlayerDetectionNoise)
	{
		//enemies should ignore noises if they're already chasing you
		if (curState == AlertStates.CHASING)
		{
			return;
		}
		else if(isPlayerDetectionNoise)
        {
            bb.Set<bool>("IsChasing", true);
            curState = AlertStates.CHASING;
			

            //do i need to do all this?
            lastKnownPlayerLocation = noiseLocation;
            agent.SetDestination(noiseLocation);
        }
		else
		{
            bb.Set<bool>("IsAlerted", true);
            agent.SetDestination(noiseLocation);
        }
	}
	private void OnDrawGizmos()
	{
		if (searchPoints.Count > 0)			//if search points are available, display all
		{
			foreach (var point in searchPoints)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawCube(point, Vector3.one * 0.1f);
			}
		}

        if (curState == AlertStates.CHASING)		//Display attack range
        {
            Gizmos.color = Color.darkRed;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
	}
}
