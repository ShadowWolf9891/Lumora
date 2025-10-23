using UnityEditorInternal;
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

    [SerializeField]
    bool isPlayerDetectionNoise = false;

    [SerializeField]
    ParticleSystem sys;

    ParticleSystem.MainModule sysMain;

    public void Awake()
    {
        sysMain = sys.main;
    }
    /// <summary>
    /// to be caled on spawn. can be overloaded to set max size on function call 
    /// </summary>
    public void SpawnNoisePing()
    {
        anim.SetTrigger("NoisePing");
        sysMain.startSize = maxSize * 3.14f;
    }
    public void SpawnNoisePing(float newMaxSize, bool setPlayerDetectionNoise)
    {
        maxSize = newMaxSize;
        isPlayerDetectionNoise = setPlayerDetectionNoise;
        SpawnNoisePing();
    }
    private void Update()
    {
        col.radius = maxSize * sizeCurrentWeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        //NOTE: Ensure enemies have an object with a working collider, rigidbody, AND enemy tag for detection! 
        if (other.CompareTag("Enemy"))
        {
            other.GetComponentInParent<EnemyBehavior>().OnHearNoise(transform.position, isPlayerDetectionNoise);
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
