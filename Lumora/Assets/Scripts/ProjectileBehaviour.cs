using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
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
}
