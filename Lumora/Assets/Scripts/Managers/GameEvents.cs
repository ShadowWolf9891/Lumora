using System;
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
/// There are also three ways to identify a specific event. Use Id if you need to know a specific entity raised event.
/// if (e is DialogueEvent)
///    // type-based handling
/// else if (e.Category == EventCategory.Dialogue)
///   // category-based fallback
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
/// The different event types for debugging specific GameEventTypes children
/// </summary>
public enum EventCategory { None, GameState, Input, Dialogue, NPCMovement, Enemy, Player, World, UI  }


/// <summary>
/// Base class for all GameEventTypes. Inherited classes must assign an EventCategory and may assign a string Id 
/// if you care about the specific instance of the event.
/// </summary>
public abstract class GameEventType
{
    public EventCategory Category { get; private set; }
	public string Id { get; private set; } // optional, can be null or data-driven
    protected GameEventType(EventCategory category, string id = null)
    {
        Category = category;
        Id = id;
    }
}

//Add derived classes of GameEventType here...

public class DialogueEvent : GameEventType
{
    public int Chapter { get; private set; }
    public int Scene { get; private set;}

    public DialogueEvent(int chapter, int scene, string id = null) : base(EventCategory.Dialogue, id)
    {
        Chapter = chapter;
        Scene = scene;
    }
}
public class NPCMovementEvent : GameEventType
{ 
    public GameObject NPCToMove {  get; private set; }
    public Transform TargetLocation { get; private set; }
	public NPCMovementEvent(GameObject npc, Transform target, string id = null) : base(EventCategory.NPCMovement, id)
	{
		NPCToMove = npc;
		TargetLocation = target;
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
    public PlayerInputEvent(PlayerInputActionType actionType, bool isPressed = false, Vector3 moveDirection = default, string id = null) : base (EventCategory.Input, id)
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
	public ChangeGameStateEvent(GameStates state, string id = null) : base(EventCategory.GameState, id)
	{
		State = state;
    }
}
public class EnterStealthEvent : GameEventType
{ 
    public EnterStealthEvent(string id = null) : base(EventCategory.Player, id){}
}
public class LeaveStealthEvent : GameEventType
{
	public LeaveStealthEvent(string id = null) : base(EventCategory.Player, id){ }
}
public class PlayerSpottedEvent : GameEventType
{
    public GameObject Spotter { get; private set; } 
    public PlayerSpottedEvent(GameObject spotter, string id = null) : base(EventCategory.Enemy, id)
    {
        Spotter = spotter;
    }
}
public class SpawnTriggerEvent : GameEventType
{
	public Vector3 Position { get; private set; }
	public float Radius { get; private set; } // optional for spherical triggers
    public bool IsRepeatable { get; private set; }
	public GameEventType EventToRaiseOnTrigger { get; private set; }
	public SpawnTriggerEvent(Vector3 position, GameEventType eventToRaiseOnTrigger, float radius = 1f, 
        bool isRepeatable = false, string id = null) : base(EventCategory.World, id)
	{
		Position = position;
		Radius = radius;
        IsRepeatable = isRepeatable;
		EventToRaiseOnTrigger = eventToRaiseOnTrigger;
    }
}

public class SpawnVisibleNoiseEvent : GameEventType 
{
    public Vector3 Position { get; private set; }
    public float MaxSize { get; private set; }
    public GameObject Noise { get; private set; }
    public SpawnVisibleNoiseEvent(GameObject noise, Vector3 position, float maxSize, string id = null) 
        : base(EventCategory.World, id) 
    {
        Position = position;
        MaxSize = maxSize;
        Noise = noise;
    }
}

public class StartQuestEvent : GameEventType
{
	public StartQuestEvent(string id) : base(EventCategory.World, id){}
}

public class ProgressQuestEvent : GameEventType
{
	public ProgressQuestEvent(string id) : base(EventCategory.World, id) { }
}








