using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(PathObjectBehavior))]
public class NPC_Behavior : MonoBehaviour
{
	[SerializeField] PathStatus curStatus = PathStatus.PAUSE;
	[SerializeField] bool stayCloseToPlayer = false;
	[SerializeField] float distanceThreshold = 5.0f;

	PathObjectBehavior pathBehavior;

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
		pathBehavior = GetComponent<PathObjectBehavior>();
    }

	private void ChangePathStatus(PathEvent e)
	{
        //Return early if irrelevent
        if (e.NPCName != gameObject.name) return;
        if (curStatus == e.NewStatus) return;

        switch (e.NewStatus)
        {
            case PathStatus.START:
				agent.SetDestination(pathBehavior.RestartPath());
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
				agent.SetDestination(pathBehavior.GoToNextPath());
				eventID = e.Id;
                break;
            case PathStatus.PREV_PATH:
				agent.SetDestination(pathBehavior.GoToPreviousPath());
				eventID = e.Id;
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
		if(pathBehavior.IsAtPoint(transform.position))
		{
			agent.SetDestination(pathBehavior.GetNextPoint());
		}

		if(pathBehavior.IsDonePath(transform.position) && eventID != null)
		{
			agent.ResetPath();
			EventManager.MarkEventCompleted(eventID);
			eventID = null;
		}
			
		if(stayCloseToPlayer) StayCloseToTarget();
	}

	private void StayCloseToTarget()
	{
		agent.isStopped = Mathf.Abs(Vector3.Distance(transform.position, playerRef.transform.position)) > distanceThreshold && !agent.isStopped;
		if(agent.isStopped ) 
		{
			previousVelocity = agent.velocity;
			agent.velocity = Vector3.zero;
		}
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
