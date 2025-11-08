using UnityEngine;

public class PlayerHealthBehaviors : MonoBehaviour
{
    [SerializeField]
    bool godModeEnabled;
    [SerializeField]
    [Range(0, 10)]
    int currentHealthValue;

    private void Start()
    {
        //TODO: Add health loading in from save file
        currentHealthValue = 10;

        GameEvents<PlayerDamagedEvent>.Subscribe(TakeDamage);
    }

 
    public void TakeDamage(PlayerDamagedEvent e)    //Triggers upon damage taken event. All damage calculation occurs within here.
    {
        if (!godModeEnabled)
        {
            currentHealthValue -= e.DamageTaken;
            GameEvents<PlayerHealthChanged>.Raise(new PlayerHealthChanged("Player Health Changed", currentHealthValue));

            if (currentHealthValue < 0)
            {
                DoGameOver();
            }
        }
    }

    public void RestoreHealth(int healingValue)     //Triggers upon health restore keybind. 
    {
        currentHealthValue = healingValue;
        GameEvents<PlayerHealthChanged>.Raise(new PlayerHealthChanged("Player Health Changed", healingValue));
    }

    private void DoGameOver()
    {
        GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent("TODO: change ID; GameStateChanged - GameOver", GameStates.Game_Over));
    }
}
