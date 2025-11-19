using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    private GameObject endScreen;

    void Start()
    {
        endScreen = GameObject.Find("EndGame");
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("Collided with player");
        	endScreen.SetActive(true);
            EventManager.Raise("Pause_Game");
        }
    }
}
