using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnCollisionEnter(Collision collision)
    {
        //checks if collision with ground layer
        if(collision.gameObject.layer == 3)
        {
            Debug.Log("Playing SFX");
            AudioManager.Instance.PlaySFX("ProjectileLanding");
        }
    }
}
