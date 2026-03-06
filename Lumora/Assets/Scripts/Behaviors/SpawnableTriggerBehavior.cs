using UnityEngine;

/// <summary>
/// This script should only be attached to the Spawnable_Trigger prefab. 
/// It raises the desired event when the player enters the spawned trigger.
/// </summary>
[RequireComponent (typeof(BoxCollider))]
public class SpawnableTriggerBehavior : MonoBehaviour
{
    private string EventToTrigger;
	private LayerMask layerMask;
	private bool IsRepeatable;
    private string Id;
    public void Initialize(string id, string eventToTrigger, LayerMask layerMask, float radius, bool isRepeatable)
    {
        EventToTrigger = eventToTrigger;
        this.layerMask = layerMask;
        IsRepeatable = isRepeatable;
        Id = id;

        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = new Vector3(radius,radius,radius);

		Debug.Log($"[SpawnTriggerBehavior] Initialized with mask={layerMask.value}");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.gameObject.layer) & layerMask.value) != 0)
		{
			Debug.Log($"{other.gameObject.name} entered the trigger.");
			EventManager.Instance.Raise(EventToTrigger);
            if(!IsRepeatable)
            {
                SpawnerManager.Instance.MarkTriggered(Id, gameObject);
            }
        }
	}
}
