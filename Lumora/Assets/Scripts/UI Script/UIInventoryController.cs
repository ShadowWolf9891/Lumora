using System;
using TMPro;
using UnityEngine;

public class UIInventoryController : MonoBehaviour
{
    //HEY!!! I'm doing the whole of inventory management here.
    //We can swap it to its own script later, but this lets me edit the UI without changing the prefab

    [Header("UI Element Refs")]
    [SerializeField]
    TextMeshProUGUI rocksHeldCounter;

    private int rocksHeld = 0;
    PlayerBehavior playerRef;

    private void OnEnable()
    {
        //on enable, grab player ref and update counter
        playerRef = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehavior>();
        rocksHeld = playerRef.rocksHeld;
        rocksHeldCounter.text = rocksHeld.ToString();

        GameEvents<CollectionEvent>.Subscribe(AddToInventory);
        GameEvents<PlayerInputEvent>.Subscribe(OnThrowRelease);
    }

    private void OnDisable()
	{
		GameEvents<CollectionEvent>.Unsubscribe(AddToInventory);
        GameEvents<PlayerInputEvent>.Unsubscribe(OnThrowRelease);
    }

	public void AddToInventory(CollectionEvent e)
    {
        Debug.Log("inventory registering");
        switch (e.Type)
        {
            case (COLLECTABLE_TYPES.LOST_CHAPTER):
                Debug.Log("inventory registering collectionevent");
                //Enable Lost Chapter UI Here
                break;

            case (COLLECTABLE_TYPES.DISTRACTION_PICKUP):
                rocksHeld += e.Count;
                rocksHeldCounter.text = rocksHeld.ToString();
                break;
        }

    }

    private void OnThrowRelease(PlayerInputEvent e)
    {
        if (e.ActionType != PlayerInputActionType.ThrowRelease)
            return;
        rocksHeld = playerRef.rocksHeld;
        rocksHeldCounter.text = rocksHeld.ToString();
    }
}
