using System;
using System.Collections.Generic;
using UnityEngine;

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
    protected GameEventType(string id, string requiredID = "", bool isCompleted = false, bool isRepeatable = false, string[] eventsToFire = null, string[] eventsOnComplete = null)
    {
        Id = id;
        RequireCompletedID = requiredID;
        IsCompleted = isCompleted;
        IsRepeatable = isRepeatable;
        EventsToFire = eventsToFire;
        EventsOnComplete = eventsOnComplete;
    }
}

//Add derived classes of GameEventType here...

public class DummyEvent : GameEventType
{
    //Good for triggering this from a trigger volume and then raising all events from EventsToFire[].
    public DummyEvent(string id) : base(id) { } 
}


#region Player Events
public enum PlayerInputActionType
{
    Move,
    Look,
    Interact,
    Crouch,
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
    public PlayerInputEvent(string id, PlayerInputActionType actionType, bool isPressed = false, Vector3 moveDirection = default) : base(id)
    {
        ActionType = actionType;
        IsPressed = isPressed;
        MoveDirection = moveDirection;
    }
}
public enum GameStates { Running, Paused, Dialogue, Cutscene, Game_Over }
public class ChangeGameStateEvent : GameEventType
{
    public GameStates State { get; private set; }
    public ChangeGameStateEvent(string id, GameStates state) : base(id)
    {
        State = state;
    }
}

public class PlayerDamagedEvent : GameEventType
{
    public int DamageTaken;
    public PlayerDamagedEvent(string id, int damageTaken) : base(id)
    {
        DamageTaken = damageTaken;
    }
}
public class PlayerHealthChanged : GameEventType //currently used to sync UI with player health
{
    public int CurrentHealthValue;
    public PlayerHealthChanged(string id, int currentHealthValue) : base(id)
    {
        CurrentHealthValue = currentHealthValue;
    }
}

public class UnlockAbilityEvent : GameEventType
{ 
    public string AbilityName;
    public UnlockAbilityEvent(string id,  string abilityName) : base(id)
    {
        AbilityName = abilityName;
    }
}

public class TeleportPlayerEvent : GameEventType
{
    Vector3 PositionToGoTo, PlayerPositionOnStart;
    public TeleportPlayerEvent(string id, Vector3 positionToGoTo, Vector3 playerPositionOnStart) : base(id) 
    {
        PositionToGoTo = positionToGoTo;
        PlayerPositionOnStart = playerPositionOnStart;
    }
}

#endregion

#region NPC and Quest Events
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
public enum PathStatus{START, PAUSE, RESUME, NEXT_PATH, PREV_PATH, END_EARLY};
public class PathEvent : GameEventType
{ 
    public string NPCName {  get; private set; }
    public PathStatus NewStatus { get; private set; }
	public PathEvent(string id, string npcName, PathStatus newStatus) : base(id)
	{
		NPCName = npcName;
		NewStatus = newStatus;
	}
}

public class StartQuestEvent : GameEventType
{
    public string QuestID { get; private set; }
    public StartQuestEvent(string id, string questID) : base(id)
    {
        QuestID = questID;
    }
}

public class ProgressQuestEvent : GameEventType
{
    public string QuestID { get; private set; }
    public ProgressQuestEvent(string id, string questID) : base(id)
    {
        QuestID = questID;
    }
}
public enum WalkType {NORMAL, PAUSE, LEAD, FOLLOW }
public class ChangeNPCWalkTypeEvent : GameEventType
{
	public string NPCName { get; private set; }
    public WalkType WalkType { get; private set; }
    public string Target { get; private set; }
    public float FollowDistance { get; private set; }

	public ChangeNPCWalkTypeEvent(string id, string npcName, WalkType mode, string target, float followDistance = 5f) : base(id)
    {
        NPCName= npcName;
        WalkType = mode;
        Target = target;
        FollowDistance = followDistance;
    }
}
#endregion

#region Item and Resource Events
public enum COLLECTABLE_TYPES
{
    LOST_CHAPTER,
    HEAL_CRYSTAL,
    DISTRACTION_CRYSTAL,
}
public class CollectionEvent : GameEventType
{
    public COLLECTABLE_TYPES Type;
    public int Count;
    public CollectionEvent(string id, COLLECTABLE_TYPES type , int count) : base(id)
    {
        Type = type;
        Count = count;
    }
}


#endregion

#region Stealth Events
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
#endregion

#region Cutscene Events
public class BeginCutsceneEvent : GameEventType
{
    public string TimelineName { get; private set; }
    public float StartTime { get; private set; }
    public float EndTime { get; private set; }
    public BeginCutsceneEvent(string id, string timelineName, float startTime = 0, float endTime = -1) : base(id)
    {
        TimelineName = timelineName;
        StartTime = startTime;
        EndTime = endTime;
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
#endregion

#region Loading Scenes
public class LoadedScene : GameEventType
{
    //note: SceneField is a custom class. Check SerializableScenesHelper class for a reference to what data it contains.
    public string SceneName{ get; private set; }
    public LoadedScene (string id, string sceneName) : base(id)
    {
        SceneName = sceneName;
    }
}
public class UnloadedScene : GameEventType
{
    //note: SceneField is a custom class. Check SerializableScenesHelper class for a reference to what data it contains.
    public string SceneName { get; private set; }
    public UnloadedScene(string id, string sceneName) : base(id)
    {
        SceneName = sceneName;
    }
}
#endregion

#region Spawning Objects
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

public class ToggleVisibilityEvent : GameEventType
{
    public string ObjectName { get; private set; }
    public bool IsVisible { get; private set; }
    public ToggleVisibilityEvent(string id, string objectName, bool isVisible) : base(id)
    {
        ObjectName = objectName;
        IsVisible = isVisible;
    }
}

#endregion

#region UI
public class SpawnPauseMenuEvent : GameEventType
{
    public bool MenuPopup { get; private set; }
    public SpawnPauseMenuEvent(string id, bool menuPopup) : base(id)
    {
        MenuPopup = menuPopup;
    }
}

#endregion