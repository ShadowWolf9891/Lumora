using System;
using TMPro;
using UnityEngine;

public class PlayerUIBIllboardBehaviors : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI interactPromptTMP;
    [SerializeField]
    GameObject interactField, crouchingPanel;
    private void Start()
    {
        GameContext.Instance.OnEnterHideState += ActivateCrouchingUI;
        GameContext.Instance.OnLeaveHideState += DeactivateCrouchingUI;
    }

    private void ActivateCrouchingUI()
    {
        crouchingPanel.SetActive(true);
    }
    private void DeactivateCrouchingUI()
    {
        crouchingPanel.SetActive(false);
    }


    void Update()
    {
        if (interactPromptTMP.text == "")
        {
            interactField.SetActive(false);
        }
        else
        {
            interactField.SetActive(true);
        }
    }
}
