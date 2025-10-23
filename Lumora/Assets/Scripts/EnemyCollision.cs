using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    [SerializeField] GameObject endScreen;
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("Collided with player");
        	endScreen.SetActive(true);
            GameEvents<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent(GameStates.Paused));
        }
    }
}
