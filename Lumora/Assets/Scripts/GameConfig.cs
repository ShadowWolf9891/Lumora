using UnityEngine;

public enum GameMode
{ 
    Production,
    Playtest
}
/// <summary>
/// Whether to use the playtest or production build.
/// </summary>
public static class GameConfig
{
    public static GameMode Mode = GameMode.Playtest;
}
