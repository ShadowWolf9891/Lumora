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
        CheckForInteractable();
    }

    /// <summary>
    /// Raycast for prompt before interacting
    /// </summary>
	private void CheckForInteractable()
	{
		//adjusted Ray to work with 3rd person movement.
		//Ray is cast in front of player.
		Ray ray = new Ray(gameObject.transform.position, gameObject.transform.forward); // Adjust origin as needed
		Debug.DrawRay(gameObject.transform.position, gameObject.transform.forward);
		if (Physics.Raycast(gameObject.transform.position, gameObject.transform.forward, out RaycastHit hit, interactRange, interactableLayer))
		{
			if (hit.collider.gameObject.TryGetComponent<IInteractable>(out var interactable))
            {
                currentInteractable = interactable;
				interactionUI.text = (currentInteractable.GetInteractionPrompt());
				return;
            }
        }
		currentInteractable = null;
		interactionUI.text = "";
	}

	public void OnInteract()
	{
		currentInteractable?.OnInteractStart();
		interactionUI.text = "";
	}


}
