using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HouseInteriorTriggerBehaviors : MonoBehaviour
{
    bool areChildrenActive = false;
    private List<GameObject> childColliders = new List<GameObject>();

    private void Start()
    {
        areChildrenActive = false;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            childColliders.Add(this.transform.GetChild(i).gameObject);
        }
        //just making sure all colliders are off on spawn
        ToggleColliders(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ToggleColliders(areChildrenActive);
            areChildrenActive ^= true;
        }
    }

    private void ToggleColliders(bool ifChildrenAreActive)
    {
        //turn outside colliders on
        if (ifChildrenAreActive)
        {
            foreach (GameObject collider in childColliders)
            {
                collider.SetActive(false);
            }
        }
        //turn outside colliders off
        else
        {
            foreach (GameObject collider in childColliders)
            {
                collider.SetActive(true);
            }
        }
    }
}
