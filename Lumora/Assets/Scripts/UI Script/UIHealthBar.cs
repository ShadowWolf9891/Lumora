using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    public static UIHealthBar Instance;
    [SerializeField]Sprite[] healthSprites;
    UnityEngine.UI.Image healthIcon;
    void Awake()
    {
        if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);

        healthIcon = GetComponent<UnityEngine.UI.Image>();
    }

	public void UpdateHealthBar(int currentHealth)
    {
        Debug.Log(currentHealth);
        if (currentHealth >= 7) //above 7
        {
            healthIcon.sprite = healthSprites[0];
        }
        else if (currentHealth <= 6 && currentHealth >= 4) //between 6 and 4
        {
            healthIcon.sprite = healthSprites[1];
        }
        else //below 3
        {
            healthIcon.sprite = healthSprites[2];
        }
    }
}
