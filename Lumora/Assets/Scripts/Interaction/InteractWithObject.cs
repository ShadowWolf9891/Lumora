using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractWithObject : MonoBehaviour
{

    IInteractable currentInteractable;
    [SerializeField] float interactRange = 2f;
    [SerializeField] LayerMask interactableLayer;
    Collider[] sphereResults = new Collider[16];
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
        GameObject closestInteracrableObj = null;
        for (int i = 0; i < count; i++)
        {
            Collider c = sphereResults[i];
            float tempDistance = Vector3.Distance(c.ClosestPoint(transform.position), transform.position);

            if (tempDistance < closestDistance)
            {
                closestDistance = tempDistance;
                closestInteracrableObj = c.gameObject;
            }
        }

        //assign closest interactable and disable prompt from other former closest
        IInteractable tempInteractable = closestInteracrableObj.GetComponentInParent<IInteractable>(); 
        if (tempInteractable != currentInteractable && tempInteractable != null && currentInteractable != null)
        {
            //Debug.Log("Scan says closest interactable has changed");
            currentInteractable.DisableInteractionPrompt();
        }
        currentInteractable = tempInteractable;

        //NOTE: this returns a string, so we can set some UI to the interaction prompt here
        currentInteractable.GetInteractionPrompt();
        Debug.Log($"InteractWithObject - TRUE, current interactable: {currentInteractable}");
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
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            return;
        }
        if (!objectInRange)
        {
            objectInRange = true;
        }

        if (CanSetClosestToCurrentInteratable())
        {

        }
    }
}
