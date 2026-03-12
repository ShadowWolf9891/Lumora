using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(PathObjectBehavior))]
public class NPC_Behavior : MonoBehaviour, ISaveable
{
	[SerializeField] PathStatus curStatus = PathStatus.PAUSE;
	[SerializeField] WalkType curWalkType = WalkType.NORMAL;
	[SerializeField] float followDistance = 5.0f;
	[SerializeField] GameObject curTarget;
	PathObjectBehavior pathBehavior;
	NavMeshAgent agent;
	Vector3 previousVelocity;
	string eventID;
	void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
		pathBehavior = GetComponent<PathObjectBehavior>();
    }
	private void OnEnable()
	{
		GameEvents<PathEvent>.Subscribe(ChangePathStatus);
		GameEvents<ChangeGameStateEvent>.Subscribe(FreezeNPC);
		GameEvents<ChangeNPCWalkTypeEvent>.Subscribe(ChangeNPCWalk);	
	}
	private void OnDisable()
	{
		GameEvents<PathEvent>.Unsubscribe(ChangePathStatus);
		GameEvents<ChangeGameStateEvent>.Unsubscribe(FreezeNPC);
		GameEvents<ChangeNPCWalkTypeEvent>.Unsubscribe(ChangeNPCWalk);
	}
	void Start()
	{
		if(curTarget == null) curTarget = GameObject.Find("Player");
	}

	private void ChangePathStatus(PathEvent e)
	{
        //Return early if irrelevent
        if (e.NPCName != gameObject.name) return;
		//if (curStatus == e.NewStatus)
		//{
		//	//EventManager.MarkEventCompleted(e.Id); 
		//	return;
		//}

		//Curstatus now is only set on Pause and Resume since those are the only states that matter.
		//All other states are just changing between the two in various circumstances.

        switch (e.NewStatus)
        {
            case PathStatus.START:
				agent.SetDestination(pathBehavior.RestartPath());
				eventID = e.Id;
				curStatus = PathStatus.RESUME;
				break;
            case PathStatus.PAUSE:
                agent.isStopped = true;
				EventManager.Instance.MarkEventCompleted(e.Id);
				curStatus = e.NewStatus;
				break;
            case PathStatus.RESUME:
                agent.isStopped = false;
				EventManager.Instance.MarkEventCompleted(e.Id);
				curStatus = e.NewStatus;
				break;
            case PathStatus.NEXT_PATH:
				agent.SetDestination(pathBehavior.GoToNextPath());
				if (eventID != null && !EventManager.Instance.GetCompletedEvents().Contains(eventID))
					EventManager.Instance.MarkEventCompleted(eventID);
				eventID = e.Id;
				curStatus = PathStatus.RESUME;
				break;
            case PathStatus.PREV_PATH:
				agent.SetDestination(pathBehavior.GoToPreviousPath());
				EventManager.Instance.MarkEventCompleted(eventID);
				eventID = e.Id;
				curStatus = PathStatus.RESUME;
				break;
            case PathStatus.END_EARLY:
                agent.isStopped = true;
                agent.ResetPath();
				EventManager.Instance.MarkEventCompleted(eventID);
				EventManager.Instance.MarkEventCompleted(e.Id);
				curStatus = PathStatus.PAUSE;
				break;
		}
	}

	private void Update()
	{
		if (curStatus == PathStatus.PAUSE) return;

		Move();
	}
	private void Move()
	{
		switch(curWalkType)
		{
			case WalkType.NORMAL : 
				MoveNPCAlongPath();
				break;
			case WalkType.LEAD:
				LeadPlayer();
				break;
			case WalkType.FOLLOW:
				FollowPlayer();
				break;
		}
	}
	private void MoveNPCAlongPath()
	{
		if (agent.isStopped) ToggleNPCMovement();

		if (!pathBehavior.HasPath()) return;
		
		//If there is no next point, reset and mark move event as complete.
		if(pathBehavior.IsDonePath(transform.position, 2f) && eventID != null)
		{
			agent.ResetPath();
			EventManager.Instance.MarkEventCompleted(eventID);
			eventID = null;
		}
		else if (pathBehavior.IsAtPoint(transform.position, 2f)) agent.SetDestination(pathBehavior.GetNextPoint()); //Go to next point if at destination
	}
	private void LeadPlayer()
	{
		if(agent.hasPath)
		{
			if (CloseToTarget(curTarget.transform.position, followDistance))
			{
				if(agent.isStopped) ToggleNPCMovement();
				MoveNPCAlongPath();
			}
			else
			{
				if (!agent.isStopped) ToggleNPCMovement();
			}
		}
	}
	private void FollowPlayer()
	{
		if (!CloseToTarget(curTarget.transform.position, followDistance))
		{
			if(!agent.hasPath || CloseToTarget(agent.destination, agent.stoppingDistance)) 
				agent.SetDestination(curTarget.transform.position);
		}
		else
		{
			if (!agent.isStopped) ToggleNPCMovement();
			agent.ResetPath();
		}

		//Warp to the player if they are too far away.
		if(agent.remainingDistance > 50)
		{
			agent.ResetPath();
			agent.Warp(curTarget.transform.position - (curTarget.transform.forward * 5));
			if(!agent.isOnNavMesh) 
			{
				agent.FindClosestEdge(out NavMeshHit hit);
				Vector3 closestPoint = hit.position;
				agent.Warp(closestPoint);
			}
			agent.SetDestination(curTarget.transform.position);
		}
	}
	
	private void ChangeNPCWalk(ChangeNPCWalkTypeEvent e)
	{
		if (e.NPCName != gameObject.name || 
			(e.WalkType == curWalkType && e.FollowDistance == followDistance && e.Target == curTarget.name)) return;

		if (e.Target != curTarget.name)
		{
			GameObject newTarget = GameObject.Find(e.Target);
			if (newTarget == null)
			{
				Debug.LogError($"Cannot find target {e.Target} in the hierarchy. " +
					$"Make sure spelling is correct and the target is visible and enabled.");
				return;
			}
		}
		curWalkType = e.WalkType;
		followDistance = e.FollowDistance;
		EventManager.Instance.MarkEventCompleted(e.Id);
	}
	/// <summary>
	/// If the NPC is close to a target position within a given threshold
	/// </summary>
	/// <param name="target">Target position</param>
	/// <param name="threshold">Valid radius from position to return true</param>
	/// <returns>If the distance from NPC to target is less than the threshold</returns>
	private bool CloseToTarget(Vector3 target, float threshold)
	{
		return Math.Abs(Vector3.Distance(agent.nextPosition, target)) <= threshold;
	}

	/// <summary>
	/// Freeze NPC when game pauses.
	/// </summary>
	/// <param name="e"></param>
	private void FreezeNPC(ChangeGameStateEvent e)
	{
		if(agent == null) return;
		if (e.State == GameStates.Running && agent.hasPath)
		{
			agent.velocity = previousVelocity;
			agent.isStopped = false;
		}
		else if (e.State != GameStates.Running) 
		{
			previousVelocity = agent.velocity;
			agent.velocity = Vector3.zero;
			agent.isStopped = true;
		}
	}
	private void ToggleNPCMovement()
	{
		agent.isStopped = !agent.isStopped;
		if (agent.isStopped)
		{
			previousVelocity = agent.velocity;
			agent.velocity = Vector3.zero;
		}
		else
		{
			agent.velocity = previousVelocity;
			agent.isStopped = false;
		}
	}

	public void Save(GameSaveData data)
	{
		if (data == null)
		{
			Debug.LogError($"Save called before GameSaveData is initialized for {name}");
			return;
		}
		data.worldData ??= new WorldSaveData();
		data.worldData.NPCData ??= new List<NPCStatusData>();

		var existing = data.worldData.NPCData.Find(x => x.InstanceId == GUID);
		if (existing != null)
		{
			existing.position = new SerializableVector3(agent.transform.position);
			existing.Status = curStatus == PathStatus.START ? PathStatus.RESUME : curStatus;
			existing.WalkType = curWalkType;
			existing.PathData.CurrentPath = pathBehavior.HasPath() ? pathBehavior.GetCurrentPathAndPoint().Item1 : -1;
			existing.PathData.CurrentPoint = pathBehavior.HasPath() ? pathBehavior.GetCurrentPathAndPoint().Item2 : -1;
			existing.ActiveEventID = eventID;
		}
		else
		{
			data.worldData.NPCData.Add(new NPCStatusData
			{
				InstanceId = GUID,
				position = new SerializableVector3(agent.transform.position),
				Status = curStatus == PathStatus.START ? PathStatus.RESUME : curStatus,
				WalkType = curWalkType,
				PathData = new PathData() 
				{ 
					CurrentPath = pathBehavior.HasPath() ? pathBehavior.GetCurrentPathAndPoint().Item1 : -1,
					CurrentPoint = pathBehavior.HasPath() ? pathBehavior.GetCurrentPathAndPoint().Item2 : -1
				},
				ActiveEventID = eventID
		});
		}
	}

	public void Load(GameSaveData data)
	{
		if (data == null) return;
		var saved = data.worldData.NPCData.Find(x => x.InstanceId == GUID);
		if (saved != null)
		{
			curStatus = saved.Status;
			curWalkType = saved.WalkType;agent.Warp(saved.position.ToVector3());
			if (saved.PathData.CurrentPath == -1 || saved.PathData.CurrentPoint == -1) return;
			Debug.Log($"Current path: {saved.PathData.CurrentPath}, Current point: {saved.PathData.CurrentPoint}");
			agent.SetDestination(pathBehavior.GoToPath(saved.PathData.CurrentPath, saved.PathData.CurrentPoint));
			eventID = saved.ActiveEventID;
		}
	}
	//Generating Unique id for saving in the editor

	[SerializeField] private string npcId;
	public string GUID => npcId;

#if UNITY_EDITOR
private void OnValidate()
{
    if (string.IsNullOrEmpty(npcId))
    {
        npcId = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
}
#endif
}
