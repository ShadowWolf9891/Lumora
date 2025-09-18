using UnityEngine;

public class CollectibleBehaviour : MonoBehaviour
{
    void OnInteract()
    {
        Destroy(this);
    }
}
