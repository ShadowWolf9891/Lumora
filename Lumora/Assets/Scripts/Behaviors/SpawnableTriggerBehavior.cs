using UnityEngine;

/// <summary>
/// This script should only be attached to the Spawnable_Trigger prefab. 
/// It raises the desired event when the player enters the spawned trigger.
/// </summary>
[RequireComponent (typeof(BoxCollider))]
public class SpawnableTriggerBehavior : MonoBehaviour
{
    private string EventToTrigger;
	private int LayerMask;
	private bool IsRepeatable;
    private string Id;
    public void Initialize(string id, string eventToTrigger, int layerMask, float radius, bool isRepeatable)
    {
        EventToTrigger = eventToTrigger;
        LayerMask = layerMask;
        IsRepeatable = isRepeatable;
        Id = id;

        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = new Vector3(radius,radius,radius);
        collider.includeLayers = LayerMask;
    }

	private void OnTriggerEnter(Collider other)
	{
        if(other.gameObject.layer == LayerMask) 
        {
            EventManager.Raise(EventToTrigger);
            if(!IsRepeatable)
            {
                EventManager.MarkEventCompleted(Id);
                Destroy(gameObject);
            }
        }
	}
}
