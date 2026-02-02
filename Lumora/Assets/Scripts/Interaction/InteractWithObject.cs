using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractWithObject : MonoBehaviour
{

    IInteractable currentInteractable;
	[SerializeField] LayerMask interactableLayer;
	[SerializeField] TextMeshProUGUI interactionUI;

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
		//GameContext.Instance.OnInteractPressed += OnInteract;
    }

    void Update()
    {
        //CheckForInteractable();
    }
    
	private void CheckForInteractable()
	{

	}

	public void OnInteract()
	{
        if (currentInteractable != null)
        {
            currentInteractable?.OnInteractStart();
            interactionUI.text = "";

            //check to see if current interactable destroyed itself?
            currentInteractable = null;
        }
        else
        {
            Debug.Log("oninteract - no current interactable");
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == interactableLayer)
        {
            Debug.Log($"ontrigger enter - {other.gameObject.name}");
            IInteractable interactable = other.gameObject.GetComponentInParent<IInteractable>();
            Debug.Log($"Current Interactable: {interactable}");
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == interactableLayer)
        {
            Debug.Log($"Current Interactable: null");
            currentInteractable = null;
        }
    }
}
