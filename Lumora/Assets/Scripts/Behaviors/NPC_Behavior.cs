using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPC_Behavior : MonoBehaviour
{
    [SerializeField] List<GameObject> gamePaths;
	[SerializeField] PathStatus curStatus = PathStatus.PAUSE;
	[SerializeField] bool stayCloseToPlayer = false;
	[SerializeField] float distanceThreshold = 5.0f;
    int currentPathIndex = 0; //Which path we are navigating
	int curPathPoint = 0; //Which point on the path is our destination
	List<WaypointPath> paths = new();
    NavMeshAgent agent;
	GameObject playerRef;
	Vector3 previousVelocity;
	string eventID;
	void Start()
    {
        GameEvents<PathEvent>.Subscribe(ChangePathStatus);
        GameEvents<ChangeGameStateEvent>.Subscribe(FreezeNPC);
        agent = GetComponent<NavMeshAgent>();
		playerRef = GameObject.Find("Player");
		foreach (GameObject go in gamePaths) 
		{
			if(go.TryGetComponent(out WaypointPath wp))
			{
				paths.Add(wp);
			}
		}
    }

	private void ChangePathStatus(PathEvent e)
	{
        //Return early if irrelevent
        if (e.NPCName != gameObject.name) return;
        if (paths == null || paths.Count == 0 || paths[currentPathIndex].points == null || paths[currentPathIndex].points.Count == 0) return;
        if (curStatus == e.NewStatus) return;

        switch (e.NewStatus)
        {
            case PathStatus.START:
                agent.SetDestination(paths[currentPathIndex].GetPointWorld(0));
				eventID = e.Id;
                break;
            case PathStatus.PAUSE:
                agent.isStopped = true;
				EventManager.MarkEventCompleted(e.Id);
				break;
            case PathStatus.RESUME:
                agent.isStopped = false;
				EventManager.MarkEventCompleted(e.Id);
				break;
            case PathStatus.NEXT_PATH:
				if (currentPathIndex < paths.Count - 1)
				{
					currentPathIndex++;
					agent.SetDestination(paths[currentPathIndex].GetPointWorld(0));
					EventManager.MarkEventCompleted(eventID);
					eventID = e.Id;
				}
                break;
            case PathStatus.PREV_PATH:
				if (currentPathIndex > 0)
				{
					currentPathIndex--;
					agent.SetDestination(paths[currentPathIndex].GetPointWorld(0));
					EventManager.MarkEventCompleted(eventID);
					eventID = e.Id;
				}
                break;
            case PathStatus.END_EARLY:
                agent.isStopped = true;
                agent.ResetPath();
				EventManager.MarkEventCompleted(eventID);
				EventManager.MarkEventCompleted(e.Id);
				break;
		}

        curStatus = e.NewStatus;
	}

	private void Update()
	{
        if(curStatus != PathStatus.PAUSE) MoveNPCAlongPath(); //Switch to behavior tree for more complex stuff.
	}

	private void MoveNPCAlongPath()
	{
		if (paths[currentPathIndex].points.Count == 0) return;

		if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
		{
			if(curPathPoint < paths[currentPathIndex].points.Count)
			{
				curPathPoint++;
				agent.SetDestination(paths[currentPathIndex].GetPointWorld(curPathPoint));
			}
			else if(paths[currentPathIndex].loop)
			{
				curPathPoint = 0;
				agent.SetDestination(paths[currentPathIndex].GetPointWorld(curPathPoint));
			}
			else
			{
				EventManager.MarkEventCompleted(eventID);
			}
		}
		StayCloseToTarget();
	}

	private void StayCloseToTarget()
	{
		agent.isStopped = Mathf.Abs(Vector3.Distance(transform.position, playerRef.transform.position)) > distanceThreshold && !agent.isStopped;
	}

	/// <summary>
	/// Freeze NPC when game pauses.
	/// </summary>
	/// <param name="e"></param>
	private void FreezeNPC(ChangeGameStateEvent e)
	{
		if (e.State == GameStates.Running)
		{
			agent.velocity = previousVelocity;
			agent.isStopped = false;
		}
		else if (e.State == GameStates.Paused || e.State == GameStates.Game_Over) //Not sure what to do with cutscenes yet
		{
			previousVelocity = agent.velocity;
			agent.velocity = Vector3.zero;
			agent.isStopped = true;
		}
	}
}
