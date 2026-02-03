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
            objectInRange = false;
            if (currentInteractable != null)
            {
                currentInteractable.DisableInteractionPrompt();
                currentInteractable = null;
            }
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

        //assign closest interactable and disable prompt from other former closest
        IInteractable tempInteractable = closestInteracrable.GetComponentInParent<IInteractable>();
        if (tempInteractable != currentInteractable && tempInteractable != null && currentInteractable != null)
        {
            //Debug.Log("Scan says closest interactable has changed");
            currentInteractable.DisableInteractionPrompt();
        }
        currentInteractable = tempInteractable;

        //NOTE: this returns a string, so we can set some UI to the interaction prompt here
        currentInteractable.GetInteractionPrompt();
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
