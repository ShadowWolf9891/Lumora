using UnityEngine;

public class DarrenMovement : MonoBehaviour
{

    [SerializeField]
    private GameObject darren;
    [SerializeField]
    private GameObject darrenDi;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("moved");
        darren.transform.position += new Vector3(0f, 0f, 2f);
        darrenDi.SetActive(true);
    }
}
