using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField]
    int currentHealth;
    Animator anim;
    void Start()
    {
        GameEvents<PlayerHealthChanged>.Subscribe(UpdateHealthBar);
        anim = GetComponent<Animator>();
    }

    private void UpdateHealthBar(PlayerHealthChanged e)
    {
        e.CurrentHealthValue = currentHealth;

        if (anim != null)
        {
            if (currentHealth >= 7)
            {
                anim.SetTrigger("GreenHealth");
            }
            else if (currentHealth <= 6 && currentHealth >= 4)
            {
                anim.SetTrigger("YellowHealth");
            }
            else //currentHealth <= 3
            {
                anim.SetTrigger("RedHealth");
            }
        }
        else
        {
            Debug.Log("Health Bar Animator is null");
        }
    }
}
