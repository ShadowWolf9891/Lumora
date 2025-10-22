using UnityEngine;

public class NoiseBehaviors : MonoBehaviour
{
    Animator anim;
    SphereCollider col;

    [SerializeField]
    float maxSize = 3f;

    [SerializeField]
    float sizeCurrentWeight = 0f;

    public void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<SphereCollider>();
    }
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
    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log($"Noise OnTriggerEnter {other.name}");
    //}
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        Debug.Log($"Noise Hit {collision.gameObject.name}");
    //    }
    //    else
    //    {
    //        Debug.Log($"Noise tagged trigger: {collision.gameObject.name}");
    //    }
    //}
    //private void OnCollisionStay(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        Debug.Log($"Noise Hit {collision.gameObject.name}");
    //    }
    //    else
    //    {
    //        Debug.Log($"Noise tagged trigger: {collision.gameObject.name}");
    //    }
    //}
    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"Noise Hit {other.gameObject.name}");
        //if (other.gameObject.CompareTag("Enemy"))
        //{
           
        //}
        //else
        //{
        //    Debug.Log($"Noise tagged trigger: {other.gameObject.name}");
        //}
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
