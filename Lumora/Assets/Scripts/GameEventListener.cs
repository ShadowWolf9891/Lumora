using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
	public GameEvent Event;
	public UnityEvent Response; //The unity event that is triggered by the scriptable object event

	private void OnEnable() => Event.Register(OnEventRaised);
	private void OnDisable() => Event.Unregister(OnEventRaised);
	private void OnEventRaised() => Response.Invoke();
}
