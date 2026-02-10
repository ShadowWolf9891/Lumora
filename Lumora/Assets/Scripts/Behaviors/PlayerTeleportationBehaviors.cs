using System;
using UnityEngine;

public class PlayerTeleportationBehaviors : MonoBehaviour
{
    //particle system
    //model reference to disable
    //
    //

    private void Start()
    {
        GameEvents<TeleportPlayerEvent>.Subscribe(DoPlayerTeleport);
    }

    private void DoPlayerTeleport(TeleportPlayerEvent e)
    {
        GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Teleporting player, pausing", GameStates.Paused));
    }
}
