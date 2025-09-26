using System;
using UnityEngine;

public class EventHandler
{
	private static EventHandler _instance;
	public static EventHandler Instance => _instance ??= new EventHandler(); //Only 1 instance
	
	private EventHandler() { }
	

}
