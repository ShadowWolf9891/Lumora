using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    float noiseMade = 5;
    void OnCollisionEnter(Collision collision)
    {
        Vector3 targetLoc = new Vector3();
        targetLoc = transform.position;
        GameContext.Instance.RaiseGenericNoise(targetLoc, noiseMade);
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
}
