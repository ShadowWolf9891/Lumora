using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public SerializableVector3 position = new();
    public SerializableVector3 rotation = new();
	public int health = 3;
    public PathData pathData = new();
    public Dictionary<string, int> inventory = new();
}
