using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.LightTransport;

public class InteractWithObject : MonoBehaviour
{

    IInteractable currentInteractable;
	[SerializeField] float interactRange = 2f;
	[SerializeField] LayerMask interactableLayer;
	[SerializeField] Collider[] sphereResults = new Collider[16];
	//[SerializeField] TextMeshProUGUI interactionUI;
	bool objectInRange;

    private void Start()
    {
		GameEvents<PlayerInputEvent>.Subscribe(e =>
		{
			if (e.ActionType == PlayerInputActionType.Interact && e.IsPressed) //Only check if the player presses the next dialogue button
			{
				OnInteract();
			}
		}
	   );
    }
    private void Update()
    {
        if (objectInRange)
		{
			CanSetClosestToCurrentInteratable();
		}
        //ensure correct object in range is highlighted / displays instructional image here? 
    }
    private bool CanSetClosestToCurrentInteratable()
    {

        int count = Physics.OverlapSphereNonAlloc(transform.position, interactRange, sphereResults, interactableLayer);

        if (count == 0)
        {
            return false;
        }

        float closestDistance = float.MaxValue;
        GameObject closestInteracrable = null;
        for (int i = 0; i < count; i++)
        {
            Collider c = sphereResults[i];
            float tempDistance = Vector3.Distance(c.ClosestPoint(transform.position), transform.position);

            if (tempDistance < closestDistance)
            {
                closestDistance = tempDistance;
                closestInteracrable = c.gameObject;
            }
        }
        currentInteractable = closestInteracrable.GetComponentInParent<IInteractable>();
        //Debug.Log($"InteractWithObject - TRUE, current interactable: {currentInteractable}");
        return true;
    }

    public void OnInteract()
	{
        if (currentInteractable == null) return;
        //Debug.Log("InteractWithObject - Running on Interact");
		currentInteractable.OnInteractStart();
	}

    private void OnTriggerEnter(Collider other)
    {
		if (!objectInRange)
		{
			objectInRange = true;
		}

        if (CanSetClosestToCurrentInteratable())
        {
            
        }
        else
        {
            Debug.LogWarning("OnTriggerEnter running on InteractWithObject.cs, but encountered error on Closest Interactable scan. Consider re-setting interact range variable");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (objectInRange)
        {
            if (CanSetClosestToCurrentInteratable())
            {
                //bool check should reset current interactable, we can do stuff here, but we shouldnt need to.
            }
            else
            {
                currentInteractable = null;
            }
        }
        else { currentInteractable = null; }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
