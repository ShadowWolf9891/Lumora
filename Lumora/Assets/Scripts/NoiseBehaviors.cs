using UnityEngine;

public class NoiseBehaviors : MonoBehaviour
{
    [SerializeField]
    Animator anim;
    [SerializeField]
    SphereCollider col;

    [SerializeField]
    float maxSize = 3f;

    [SerializeField]
    float sizeCurrentWeight = 0f;

    /// <summary>
    /// to be caled on spawn. can be overloaded to set max size on function call 
    /// </summary>
    public void SpawnNoisePing()
    {
        anim.SetTrigger("NoisePing");
    }
    public void SpawnNoisePing(float newMaxSize)
    {
        maxSize = newMaxSize;
        anim.SetTrigger("NoisePing");
    }
    private void Update()
    {
        col.radius = maxSize * sizeCurrentWeight;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Noise Hit an Enemy!");
        }
        else
        {
            Debug.Log("asdfasdfasdf");
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    //
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.lawnGreen;
        Gizmos.DrawWireSphere(transform.position, maxSize * sizeCurrentWeight);
    }
}
