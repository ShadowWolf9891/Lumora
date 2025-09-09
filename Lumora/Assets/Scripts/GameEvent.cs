using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/GameEvent")]
public class GameEvent : ScriptableObject
{
    private event Action OnEventRaised;
    public void Raise() =>OnEventRaised?.Invoke();
	public void Register(Action listener) => OnEventRaised += listener;
	public void Unregister(Action listener) => OnEventRaised -= listener;
}
