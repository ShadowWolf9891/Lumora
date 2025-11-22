using UnityEngine;

public class PulseSize : MonoBehaviour
{
    public float speed = 2f;         
    public float amount = 0.1f;     

    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float pulse = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        transform.localScale = baseScale * (1 + pulse * amount);
    }
}
