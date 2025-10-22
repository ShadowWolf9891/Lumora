using UnityEngine;

public class MrsWattsMovement : MonoBehaviour
{
    [SerializeField]
    private GameObject watts;
    [SerializeField]
    private GameObject wattsDi;
    //public Vector3 position;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //watts = GetComponent<GameObject>();
        //position = watts.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("moved");
        watts.transform.position += new Vector3(2f, 0, 0);
        wattsDi.SetActive(true);
    }
}
