using UnityEngine;

public class PlayerHealthBehaviors : MonoBehaviour
{
    [Header("Health Values")]
    [SerializeField]
    [Range(0, 10)]
    int maxHealth;
    [SerializeField]
    int currentHealthValue;
    [SerializeField]
    bool godModeEnabled;

    private void Start()
    {
        //TODO: Add health loading in from save file
        currentHealthValue = 0;
        RestoreHealth(100);

        GameEvents<PlayerDamagedEvent>.Subscribe(TakeDamage);
    }

 
    public void TakeDamage(PlayerDamagedEvent e)    //Triggers upon damage taken event. All damage calculation occurs within here.
    {
        if (!godModeEnabled)
        {
            currentHealthValue -= e.DamageTaken;

            if (currentHealthValue < 0)
            {
                DoGameOver();
            }
            GameEvents<PlayerHealthChanged>.Raise(new PlayerHealthChanged("Player Health Changed", currentHealthValue));
        }
    }

    public void RestoreHealth(int healingValue)     //Triggers upon health restore keybind. should probably be tied to animation event?
    {
        currentHealthValue += healingValue;
        if (currentHealthValue > maxHealth) { currentHealthValue = maxHealth; }
        GameEvents<PlayerHealthChanged>.Raise(new PlayerHealthChanged("Player Health Changed", currentHealthValue));
    }

    private void DoGameOver()       //Triggers Game Over state change. we should have GameManager do some kinda event for game over methinks
    {
        Debug.Log("Triggered Game over!");
        GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("TODO: change ID; GameStateChanged - GameOver", GameStates.Game_Over));
    }
}
