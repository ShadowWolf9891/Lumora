using System;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int version = 1;
    public PlayerSaveData playerData;
    public WorldSaveData worldData;
    public EventSaveData eventData;
}
