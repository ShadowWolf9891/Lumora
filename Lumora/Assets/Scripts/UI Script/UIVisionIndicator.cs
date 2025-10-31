using System;
using UnityEngine;

public class UIVisionIndicator : MonoBehaviour
{
    [SerializeField]
    GameObject attachedGraphic;

    private void Start()
    {
        GameEvents<PlayerSpottedEvent>.Subscribe(EnableGraphic);
    }

    private void EnableGraphic(PlayerSpottedEvent e)
    {
        attachedGraphic.SetActive(true);
    }

}
