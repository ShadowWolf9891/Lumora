using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField]
    int currentHealth;
    Animator anim;
    void Awake()
    {
        
        anim = GetComponent<Animator>();
    }
	private void OnEnable()
	{
		GameEvents<PlayerHealthChanged>.Subscribe(UpdateHealthBar);
	}
	private void OnDisable()
	{
		GameEvents<PlayerHealthChanged>.Unsubscribe(UpdateHealthBar);
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
