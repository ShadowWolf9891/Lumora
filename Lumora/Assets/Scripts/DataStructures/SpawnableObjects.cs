using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableObjects", menuName = "Scriptable Objects/SpawnableObjects")]
public class SpawnableObjects : ScriptableObject
{
    public List<GameObject> objectList;
}
