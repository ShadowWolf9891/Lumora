using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interface to attach to an interactable object that the player can interact with.
public interface IInteractable
{
	string GetInteractionPrompt(); //"Press 'E' to open"
	bool IsHoldInteraction(); //Should the button be held down to interact
	void OnInteractStart(); //When the button is first held down
	void OnInteractStop(); //When the button is released or canceled

}
