using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Use this class to raise an event of a GameEventType T. 
/// Usage raising event: GameEvent<DialogueEvent>.Raise(new DialogueEvent(1,2));
/// Usage listening to event: GameEvents<DialogueEvent>.Subscribe(YourSubroutine);
/// Note:
///       YourSubroutine must reference the event, or use e=>YourSubroutine while subscribing.
/// Example: private void YourSubroutine(DialogueEvent e)
///         {
///             int chapter = e.Chapter;
///             int scene = e.Scene;
///         }
/// There are also two ways to identify a specific event. Use Id if you need to know a specific entity raised event.
/// if (e is DialogueEvent)
///    // type-based handling
/// else if (e.Id == "dialogue_intro")
///    // data-driven matching
/// 
/// </summary>
/// <typeparam name="T">A new GameEventType with parameters for that type</typeparam>
public static class GameEvents<T> where T : GameEventType
{
    private static event Action<T> OnEventRaised;
    public static void Raise(T evt) => OnEventRaised?.Invoke(evt);
	public static void Subscribe(Action<T> listener)
	{
		OnEventRaised += listener;
	}

	public static void Unsubscribe(Action<T> listener)
	{
		OnEventRaised -= listener;
    }
}

/// <summary>
/// Intermediate class for converting json file data to game events.
/// </summary>
[System.Serializable]
public class GameEventDefinition
{
    public string type;
    public string id;
    public bool isCompleted = false;
    public string requireCompletedID;
    public string[] eventsToFire;
    public string[] eventsOnComplete;
	public Dictionary<string, string> parameters { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// Base class for all GameEventTypes. Inherited classes must assign a string Id to keep track of the event.
/// </summary>
public abstract class GameEventType
{
	public string Id { get; private set; }
    public bool IsCompleted;
    public bool IsRepeatable;
    public string RequireCompletedID;
    public string[] EventsToFire;
    public string[] EventsOnComplete;
    protected GameEventType(string id, string requiredID = "", bool isCompleted = false, bool IsRepeatable = false, string[] eventsToFire = null, string[] eventsOnComplete = null)
    {
        Id = id;
        RequireCompletedID = requiredID;
        IsCompleted = isCompleted;
        EventsToFire = eventsToFire;
        EventsOnComplete = eventsOnComplete;
    }
}

//Add derived classes of GameEventType here...

public class DialogueEvent : GameEventType
{
    public int Chapter { get; private set; }
    public int Scene { get; private set;}

    public DialogueEvent(string id, int chapter, int scene) : base(id)
    {
        Chapter = chapter;
        Scene = scene;
    }
}
public class NPCMovementEvent : GameEventType
{ 
    public string NPCToMove {  get; private set; }
    public Vector3 TargetLocation { get; private set; }
	public Vector3 TargetRotation { get; private set; }
	public NPCMovementEvent(string id, string npc, Vector3 targetLocation, Vector3 targetRotation) : base(id)
	{
		NPCToMove = npc;
		TargetLocation = targetLocation;
		TargetRotation = targetRotation;
	}
}

public enum PlayerInputActionType
{
    Move,
    Look,
	Interact,
	Hide,
    Sprint,
	Jump,
	Throw,
	ThrowRelease,
    NextDialogue
}
public class PlayerInputEvent : GameEventType
{ 
    public PlayerInputActionType ActionType { get; private set; }
    public bool IsPressed { get; private set; } //For buttons
    public Vector3 MoveDirection { get; private set; } //For movement
    public PlayerInputEvent(string id, PlayerInputActionType actionType, bool isPressed = false, Vector3 moveDirection = default) : base (id)
	{
		ActionType = actionType;
        IsPressed = isPressed;
        MoveDirection = moveDirection;
    }
}
public enum GameStates {Running, Paused, Dialogue,Cutscene}
public class ChangeGameStateEvent : GameEventType
{
    public GameStates State { get; private set; }
	public ChangeGameStateEvent(string id, GameStates state) : base(id)
	{
		State = state;
    }
}
public class EnterStealthEvent : GameEventType
{ 
    public EnterStealthEvent(string id) : base(id){}
}
public class LeaveStealthEvent : GameEventType
{
	public LeaveStealthEvent(string id) : base(id){ }
}
public class PlayerSpottedEvent : GameEventType
{
    public GameObject Spotter { get; private set; } 
    public PlayerSpottedEvent(string id,GameObject spotter) : base(id)
    {
        Spotter = spotter;
    }
}

public class EnemyDropsAlert : GameEventType
{
    public GameObject Enemy {  get ; private set; }
    public EnemyDropsAlert(string id, GameObject enemy) : base(id)
    {
        Enemy = enemy;
    }
}

public class SpawnObjectEvent : GameEventType
{
    public string PrefabName { get; private set; }
	public Vector3 Position { get; private set; }
	public Vector3 Rotation { get; private set; }

    public SpawnObjectEvent(string id, string prefabName, Vector3 position, Vector3 rotation = new Vector3()) : base(id)
    {
        PrefabName = prefabName;
        Position = position;
        Rotation = rotation;
    }
}

public class SpawnTriggerEvent : GameEventType
{
	public Vector3 Position { get; private set; }
	public float Radius { get; private set; } // optional for spherical triggers
    public LayerMask layerMask { get; private set; }
	public string EventToRaiseOnTrigger { get; private set; }
	public SpawnTriggerEvent(string id,Vector3 position, string eventToRaiseOnTrigger, LayerMask layerMask,
        float radius = 1f) : base(id)
	{
		Position = position;
		Radius = radius;
		EventToRaiseOnTrigger = eventToRaiseOnTrigger;
        this.layerMask = layerMask;
    }
}

public class SpawnVisibleNoiseEvent : GameEventType 
{
    public Vector3 Position { get; private set; }
    public float MaxSize { get; private set; }
    public bool IsPlayerSpecificNoise{ get; private set; }
    public SpawnVisibleNoiseEvent(string id, bool isPlayerSpecificNoise, Vector3 position, float maxSize) 
        : base(id) 
    {
        Position = position;
        MaxSize = maxSize;
        IsPlayerSpecificNoise = isPlayerSpecificNoise;
    }
}

public class CameraMoveEvent : GameEventType 
{
    public Vector3 TargetLocation { get; private set; }
    public Vector3 WorldLocation { get; private set; }
    public float MoveSpeed { get; private set; }
    public bool AutoReturn { get; private set; }

    public CameraMoveEvent(string id, Vector3 targetLocation, Vector3 worldLocation, float moveSpeed = 1, bool autoReturn = false) : base(id)
	{
		TargetLocation = targetLocation;
		WorldLocation = worldLocation;
		MoveSpeed = moveSpeed;
		AutoReturn = autoReturn;
	}
}
public class CameraPanEvent : GameEventType
{
	public Vector3 TargetRotation { get; private set; }
	public Vector3 WorldRotation { get; private set; }
	public float RotationSpeed { get; private set; }
	public bool AutoReturn { get; private set; }

	public CameraPanEvent(string id, Vector3 targetRotation, Vector3 worldRotation, float rotationSpeed = 1, bool autoReturn = false) : base(id)
	{
		TargetRotation = targetRotation;
		WorldRotation = worldRotation;
		RotationSpeed = rotationSpeed;
		AutoReturn = autoReturn;
	}
}
public class StartQuestEvent : GameEventType
{
	public StartQuestEvent(string id) : base(id) { }
}

public class ProgressQuestEvent : GameEventType
{
	public ProgressQuestEvent(string id) : base(id) { }
}




