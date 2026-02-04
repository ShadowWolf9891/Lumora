using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interface to attach to an interactable object that the player can interact with.
public interface IInteractable
{
	string GetInteractionPrompt(); //"Press 'E' to open"
	void DisableInteractionPrompt();
	bool IsHoldInteraction(); //Should the button be held down to interact
	bool OnInteractStart(); //When the button is first held down, RETURN TRUE IF OBJECT DESTROYS ITSELF!! <3
	void OnInteractStop(); //When the button is released or canceled

}
