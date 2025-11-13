using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField]
    int currentHealth;
    Animator anim;
    void Awake()
    {
        GameEvents<PlayerHealthChanged>.Subscribe(UpdateHealthBar);
        anim = GetComponent<Animator>();
    }

    private void UpdateHealthBar(PlayerHealthChanged e)
    {
        currentHealth = e.CurrentHealthValue;

        if (anim != null)
        {
            if (currentHealth >= 7) //above 7
            {
                anim.SetTrigger("GreenHealth");
            }
            else if (currentHealth <= 6 && currentHealth >= 4) //between 6 and 4
            {
                anim.SetTrigger("YellowHealth");
            }
            else //below 3
            {
                anim.SetTrigger("RedHealth");
                Debug.Log("Health Bar is in red");
            }
        }
        else Debug.Log("Health Bar Animator is null");
    }
}
