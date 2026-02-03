using UnityEngine;

public class UIInventoryController : MonoBehaviour
{
    //HEY!!! I'm doing the whole of inventory management here.
    //We can swap it to its own script later, but this lets me edit the UI without changing the prefab

    [SerializeField]
    GameObject bG;

    private void Start()
    {
        GameEvents<CollectionEvent>.Subscribe(AddToInventory);
    }

    public void AddToInventory(CollectionEvent e)
    {
        Debug.Log("inventory registering");
        switch (e.Type)
        {
            case (COLLECTABLE_TYPES.LOST_CHAPTER):
                Debug.Log("inventory registering collectionevent");
                bG.SetActive(true);
                break;
            
        }
    }
}
