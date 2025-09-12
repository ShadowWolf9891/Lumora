using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueEvent", menuName = "Events/DialogueEvent")]
public class DialogueEvent : ScriptableObject
{
	private event Action<int, int> OnEventRaised;
	public void Raise(int chapter, int scene) => OnEventRaised?.Invoke(chapter, scene);
	public void Register(Action<int, int> listener) => OnEventRaised += listener;
	public void Unregister(Action<int, int> listener) => OnEventRaised -= listener;
}