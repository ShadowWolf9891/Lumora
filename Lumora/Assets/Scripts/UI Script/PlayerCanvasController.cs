using UnityEngine;

public class PlayerCanvasController : MonoBehaviour
{
    //Health UI
    [SerializeField] int currentHealth;
    [SerializeField] GameObject healthBarUI;
    Animator healthBarAnim;

    void Awake()
    {
        GameEvents<PlayerHealthChanged>.Subscribe(UpdateHealthBar);
        healthBarAnim = healthBarUI.GetComponent<Animator>();
    }



    private void UpdateHealthBar(PlayerHealthChanged e)
    {
        currentHealth = e.CurrentHealthValue;

        if (healthBarAnim != null)
        {
            if (currentHealth >= 7) //above 7
            {
                healthBarAnim.SetTrigger("GreenHealth");
            }
            else if (currentHealth <= 6 && currentHealth >= 4) //between 6 and 4
            {
                healthBarAnim.SetTrigger("YellowHealth");
            }
            else //below 3
            {
                healthBarAnim.SetTrigger("RedHealth");
                Debug.Log("Health Bar is in red");
            }
        }
        else Debug.Log("Health Bar Animator is null");
    }
}
