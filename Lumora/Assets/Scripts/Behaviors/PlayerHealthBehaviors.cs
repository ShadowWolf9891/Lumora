using UnityEngine;

public class PlayerHealthBehaviors : MonoBehaviour
{
    [Header("Health Values")]
    [SerializeField]
    [Range(0, 10)]
    int maxHealth = 3;
    public int CurrentHealthValue { get; set; }
    [SerializeField]
    bool godModeEnabled;

    private void Start()
    {
        //Todo? Add health value from save on start
        CurrentHealthValue = maxHealth;
        GameEvents<PlayerDamagedEvent>.Subscribe(TakeDamage);
    }

 
    public void TakeDamage(PlayerDamagedEvent e)    //Triggers upon damage taken event. All damage calculation occurs within here.
    {
        if (!godModeEnabled)
        {
            CurrentHealthValue -= e.DamageTaken;

            if (CurrentHealthValue < 0)
            {
                DoGameOver();
            }
            GameEvents<PlayerHealthChanged>.Raise(new PlayerHealthChanged("Player Health Changed", CurrentHealthValue));
        }
    }

    public void RestoreHealth(int healingValue)     //Triggers upon health restore keybind. should probably be tied to animation event?
    {
        CurrentHealthValue += healingValue;
        if (CurrentHealthValue > maxHealth) { CurrentHealthValue = maxHealth; }
        GameEvents<PlayerHealthChanged>.Raise(new PlayerHealthChanged("Player Health Changed", CurrentHealthValue));
    }

    private void DoGameOver()       //Triggers Game Over state change. we should have GameManager do some kinda event for game over methinks
    {
        Debug.Log("Triggered Game over!");
        GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("TODO: change ID; GameStateChanged - GameOver", GameStates.Game_Over));
    }
}
