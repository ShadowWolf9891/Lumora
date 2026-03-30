using UnityEngine;

public class BillboardFaceCamera : MonoBehaviour
{
    //Helper script to face objects towards the main camera. Should allow 2D objects to 'exist' in 3D space (canvases, any image)
    Transform cameraTarget;
    private void Start()
    {
        cameraTarget = Camera.main.transform;
    }
    void Update()
    {
        if (cameraTarget != CameraManager.Instance.CurrentCamera.transform) 
        {
            cameraTarget = CameraManager.Instance.CurrentCamera.transform;
        }
        Vector3 pointToLookAt = transform.position + (transform.position - cameraTarget.position);
        transform.LookAt(pointToLookAt);
    }
}
