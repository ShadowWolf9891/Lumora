using System;
using UnityEngine;

[RequireComponent(typeof(PlayerBehavior), typeof(Rigidbody))]
public class PlayerTeleportationBehaviors1 : MonoBehaviour
{
    //we're assuming this float is equivalent to distance per second. im pretty sure i have to adjust
    [SerializeField]
    float teleportMoveSpeed;

    [SerializeField]
    GameObject modelObject;
    [SerializeField]
    GameObject particleObject;


    Rigidbody playerRigidBody;
    //PlayerBehavior playerBehaviorRef;

    private void Awake()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        //playerBehaviorRef = GetComponent<PlayerBehavior>();
        if (particleObject.activeSelf)
        {
            particleObject.SetActive(false);
        }
    }
    private void Start()
    {
        GameEvents<TeleportPlayerEvent>.Subscribe(DoTeleport);
    }

    private void DoTeleport(TeleportPlayerEvent e)
    {
        GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Teleporting!, changing game state to Cutscene", GameStates.Teleporting));

        EnableTeleportSettings(true);

        Vector3 differenceBetweenPoints = -(transform.position - e.PositionToGoTo);
        playerRigidBody.linearVelocity = differenceBetweenPoints.normalized * teleportMoveSpeed;

        Invoke("EndTeleport", differenceBetweenPoints.magnitude / teleportMoveSpeed);
    }

    private void EndTeleport()
    {
        EnableTeleportSettings(false);

        GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("Ending Teleport!, changing game state to Running", GameStates.Running));
    }

    private void EnableTeleportSettings(bool shouldBeTeleporting)
    {
        //disable model, enable particles
        modelObject.SetActive(!shouldBeTeleporting);
        particleObject.SetActive(shouldBeTeleporting);

        //set rigidbody to teleport settings, set movement 
        playerRigidBody.useGravity = !shouldBeTeleporting;
    }
}
