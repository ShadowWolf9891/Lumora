using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    private TextMeshProUGUI dialogueText;
    private string fullText;
    public float characterDelay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueText = GetComponent<TextMeshProUGUI>();
        if(dialogueText != null)
        {
            fullText = dialogueText.text;
            dialogueText.text = "";
            StartCoroutine(ShowText());
        }
        else
        {
            Debug.LogError("TMP componenet not found");
        }
    }
    IEnumerator ShowText()
    {
        for(int i = 0; i <= fullText.Length; i++)
        {
            //sets number of visible characters
            dialogueText.maxVisibleCharacters = i;
            //waits for delay before showing next character
            yield return new WaitForSeconds(characterDelay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
