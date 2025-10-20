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
            AudioManager.Instance.PlaySFX("ProjectileLanding");
            Destroy(gameObject, 0.1f);
        }
        else
        {
            AudioManager.Instance.PlaySFX("ProjectileLanding");
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        GameContext.Instance.RaiseGenericNoise(transform.position, noiseMade);
    }
}
