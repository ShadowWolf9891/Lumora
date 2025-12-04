using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    float noiseMade = 5;
    void OnCollisionEnter(Collision collision)
    {
        //checks if collision with ground layer
        if (collision.gameObject.layer == 3)
        {
            AudioManager.Instance.PlaySFX("S_Distraction_1");
            Destroy(gameObject, 0.1f);
        }
        else
        {
            AudioManager.Instance.PlaySFX("S_Distraction_2");
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        //TODO: Use something other than gameObject for the spawned noise.
		GameEvents<SpawnVisibleNoiseEvent>.Raise(new SpawnVisibleNoiseEvent("VisibleNoise", false, transform.position, noiseMade));
		//GameContext.Instance.RaiseGenericNoise(transform.position, noiseMade);
    }
}
