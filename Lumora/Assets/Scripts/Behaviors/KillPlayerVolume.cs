using UnityEngine;

public class KillPlayerVolume : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			EventManager.Raise("GameOver");
		}
	}
}
