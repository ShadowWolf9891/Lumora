using UnityEngine;
using EasyBehaviorTree;
using UnityEngine.AI;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem.XR;
using System.Collections;

[RequireComponent(typeof(PathObjectBehavior))]
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
	[SerializeField]
	Vector3 eyeLocation = new Vector3(0,1.6f,0);

	[Header("Patrol Points")]
	[SerializeField]
	float timeAtEachPatrolPoint = 2f;


	[Header("Search Settings")]
	[SerializeField] float searchRadius = 10f;
	[SerializeField] int numberOfSearchPoints = 3;
	[SerializeField] float timeAtEachPoint = 1f;
	[SerializeField] float lookRotationSpeed = 90f;   // degrees per second
	[SerializeField] float[] lookAngles = { -45f, 45f, -20f }; // angles to sweep through


	[Header("Alerted Properties")]
	[SerializeField]
	float alertedTime = 2f;

	[Header("Attack Damage")]
	[SerializeField]
	[Range(1, 10)]
	public int attackDamage = 4;

	[Header("Attack Lockout Time")]
	[SerializeField]
	public float attackLockoutTime = 1f;

    NavMeshAgent agent;
	GameObject playerRef;
	BehaviorTree bt;
	BTBlackboard bb;
	PathObjectBehavior pathBehavior;

	EnemyAlertController alertController;
	Vector3 lastKnownPlayerLocation;
	AlertStates curState;
	float waitTimer = 0; //Amount of time to wait at each patrol point.
	List<Vector3> searchPoints = new(); //List of points generated when lost sight of player
	Vector3 previousVelocity; //Before / after freezing

	string bb_CanSeePlayer = "CanSeePlayer";
	string bb_IsAlerted = "IsAlerted";
	string bb_LostPlayer = "LostPlayer";

	private bool isLooking = false;
	private Quaternion rotationBeforeLook; // Store as a field, not a local
	private float previousAngularSpeed;
	private Coroutine lookCoroutine;       // Track the coroutine so you can stop it reliably

	//[SerializeField] GameObject endScreen;
	EnemyAnimatorController animController;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		playerRef = GameObject.FindGameObjectWithTag("Player");
		animController = GetComponentInChildren<EnemyAnimatorController>();
		pathBehavior = GetComponent<PathObjectBehavior>();
		agent.SetDestination(pathBehavior.RestartPath());

		bb = ScriptableObject.CreateInstance<BTBlackboard>();
		bt = DeepCloneBehaviorTree(btAsset);
		
		// Initialize blackboard values
		bb.Set<bool>(bb_CanSeePlayer, false);
		bb.Set<bool>(bb_IsAlerted, false);
		bb.Set<bool>(bb_LostPlayer, false);

		alertController = GetComponentInChildren<EnemyAlertController>();
		curState = AlertStates.IDLE;
		OnChangeState();

		//GameContext.Instance.OnPauseGame += FreezeEnemy;
		//GameContext.Instance.OnUnPauseGame += UnFreezeEnemy;
	}
	private void OnEnable()
	{
		GameEvents<ChangeGameStateEvent>.Subscribe(FreezeEnemy);
	}
	private void OnDisable()
	{
		GameEvents<ChangeGameStateEvent>.Unsubscribe(FreezeEnemy);
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
		if (playerRef == null)
		{
			return;
		}

		float distance = Vector3.Distance(transform.position, playerRef.transform.position);
		bool canSee = false;

		// Only do expensive raycast if within alert range
		if (Mathf.Abs(distance) <= alertRange)
		{
			canSee = CanSeeTarget(playerRef, alertRange);
			if (canSee && !bb.Get<bool>(bb_IsAlerted))
			{
				bb.Set<bool>(bb_IsAlerted, true);
				if (agent.destination == null)
				{
					Debug.LogWarning("Enemy can see player, but has no target");
				}
			}
		}

		if (distance <= chasingRange && canSee)
		{
			if (!bb.Get<bool>(bb_CanSeePlayer)) bb.Set<bool>(bb_CanSeePlayer, true);
		}
		else
		{
			if (bb.Get<bool>(bb_CanSeePlayer))
			{
				bb.Set<bool>(bb_CanSeePlayer, false);
				if (!bb.Get<bool>(bb_LostPlayer)) bb.Set<bool>(bb_LostPlayer, true);
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
		if (!pathBehavior.HasPath()) return;

		if (pathBehavior.IsAtPoint(transform.position))
		{
			waitTimer += Time.deltaTime;
			if (waitTimer >= timeAtEachPatrolPoint)
			{
				agent.SetDestination(pathBehavior.GetNextPoint());
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
		//TODO: Have enemy look around
		waitTimer += Time.deltaTime;
		if (waitTimer >= alertedTime) //Wait an amount of time alerted
		{
			waitTimer = 0f;
			agent.SetDestination(pathBehavior.GetNextPoint());

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
		if (curState != AlertStates.ALERT)
		{
			curState = AlertStates.ALERT;
			OnChangeState();
		}

		if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
		{
			if (!isLooking) lookCoroutine = StartCoroutine(LookAroundAtPoint());

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

		Vector3 predictedPos = playerRef.transform.position + playerRef.GetComponent<Rigidbody>().linearVelocity;

		agent.SetDestination(predictedPos);

		lastKnownPlayerLocation = predictedPos;

		if (IsObjectInRange(playerRef, attackRange))
		{
			bb.Set("IsAttacking", true);
		}
	}

	public void Attack()
	{
		animController.DoAttack();
		agent.destination = transform.position;
		//Should lock enemy movement until AnimationController runs EndAttackState,
		//which gets triggered at the end of the attack animation.
	}
	public void EndAttackState()
	{
		bb.Set("IsAttacking", false);
	}
	#endregion

	private void OnChangeState()
	{
		StopLooking();
		alertController.ChangeImage(curState);
		searchPoints.Clear();
	}

	private void FreezeEnemy(ChangeGameStateEvent e) 
	{
		if(e.State == GameStates.Running || e.State == GameStates.Teleporting)
		{
			agent.velocity = previousVelocity;
			agent.isStopped = false;
			animController.animator.speed = 1;
		}
		else if (e.State != GameStates.Running && e.State != GameStates.Teleporting)
		{
			previousVelocity = agent.velocity;
			agent.velocity = Vector3.zero;
			agent.isStopped = true;
            animController.animator.speed = 0;
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
        return VisionHelper.CanSeeTarget(gameObject, target,  range, angleOfVision, eyeLocation, LayerMask.GetMask("Default", "Player", "Obstacles"));
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
		float closestDistance = Vector3.Distance(points[0], startPos);

		foreach (Vector3 point in points)
		{
			float dist = Vector3.Distance(point, startPos);
			if (dist < closestDistance)
			{
				closestDistance = dist;
				closestPoint = point;
			}
		}
		return closestPoint;
	}
	private IEnumerator LookAroundAtPoint()
	{
		isLooking = true;
		previousAngularSpeed = agent.angularSpeed;
		agent.angularSpeed = 0f;
		rotationBeforeLook = transform.rotation; // Capture once into the field

		foreach (float angle in lookAngles)
		{
			Quaternion targetRotation = rotationBeforeLook * Quaternion.Euler(0, angle, 0);
			while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
			{
				if (!agent.isStopped)
					transform.rotation = Quaternion.RotateTowards(
						transform.rotation,
						targetRotation,
						lookRotationSpeed * Time.deltaTime
					);
				yield return null;
			}
			yield return new WaitForSeconds(0.2f);
		}

		// Restore rotation on natural completion
		yield return StartCoroutine(ReturnToBaseRotation(previousAngularSpeed));
	}
	private void StopLooking()
	{
		if (lookCoroutine != null)
		{
			StopCoroutine(lookCoroutine);
			lookCoroutine = null;
		}
		if (isLooking)
		{
			transform.rotation = rotationBeforeLook; // Snap back on interruption
			agent.angularSpeed = previousAngularSpeed; // Restore agent control
			isLooking = false;
		}
	}

	private IEnumerator ReturnToBaseRotation(float previousAngularSpeed)
	{
		while (Quaternion.Angle(transform.rotation, rotationBeforeLook) > 0.5f)
		{
			if (!agent.isStopped)
				transform.rotation = Quaternion.RotateTowards(
					transform.rotation,
					rotationBeforeLook,
					lookRotationSpeed * Time.deltaTime
				);
			yield return null;
		}

		transform.rotation = rotationBeforeLook;
		agent.angularSpeed = previousAngularSpeed;
		isLooking = false;
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
	public void OnHearNoise(Vector3 noiseLocation)
	{
		Debug.Log($"{gameObject.name} heard Noise. Alert state = {curState}");

		//enemies should ignore noises if they're already chasing you
		if (curState == AlertStates.CHASING)
		{
			return;
		}
		else
        {
			bb.Set<bool>(bb_IsAlerted, true);
			agent.SetDestination(noiseLocation);
        }
		
	}
	private void OnDrawGizmos()
	{
		if (searchPoints.Count > 0)         //if search points are available, display all
		{
			foreach (var point in searchPoints)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawCube(point, Vector3.one * 0.1f);
			}
		}

		if (curState == AlertStates.CHASING)        //Display attack range
		{
			Gizmos.color = Color.darkRed;
			Gizmos.DrawWireSphere(transform.position, attackRange);
		}
		float viewDistance = alertRange;
		if (playerRef != null)
		{ 
			if (playerRef.TryGetComponent<VisibilityManager>(out VisibilityManager vm))
			{
				viewDistance -= viewDistance * vm.Visibility;
			}
		}
		Gizmos.color = Color.yellow;
		Quaternion rotation = Quaternion.Euler(0, angleOfVision / 2, 0);
		Vector3 direction = rotation * transform.forward * viewDistance;
		Gizmos.DrawLine(transform.position + eyeLocation, transform.position + eyeLocation + direction );
		Quaternion rotation2 = Quaternion.Euler(0, -angleOfVision / 2, 0);
		Vector3 direction2 = rotation2 * transform.forward * viewDistance;
		Gizmos.DrawLine(transform.position + eyeLocation, transform.position + eyeLocation + direction2);
		Gizmos.DrawLine(transform.position + eyeLocation, transform.position + eyeLocation + transform.forward * viewDistance);

	}
}
