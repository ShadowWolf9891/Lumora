using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour 
{
	public static InventoryManager Instance { get; private set; }
	public Dictionary<string, int> InventoryData { get; set; }
	readonly HashSet<InventoryObject> itemData;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else Destroy(gameObject);
	}
	/// <summary>
	/// Add an inventory object to the player's inventory. Increase amount by 1 if they already have it.
	/// </summary>
	/// <param name="io">The object to add.</param>
	public void Add(InventoryObject io)
	{
		if(!itemData.Contains(io)) itemData.Add(io);
		if(InventoryData.ContainsKey(io.itemName))InventoryData[io.itemName] += 1;
		else InventoryData.Add(io.itemName, 1);
	}
	/// <summary>
	/// Remove an inventory object from the player's inventory. If they have more than 1, decrease the amount by 1.
	/// </summary>
	/// <param name="io">The object to remove.</param>
	public void Remove(InventoryObject io)
	{
		if (itemData.Contains(io)) itemData.Remove(io);
		if (InventoryData.ContainsKey(io.itemName)) InventoryData[io.itemName] -= 1;
		if (InventoryData[io.itemName] <= 0) InventoryData.Remove(io.itemName);
	}
	/// <summary>
	/// Gets the item data given a string name of the item. Does not tell you the amount in the inventory, only that it exists.
	/// </summary>
	/// <param name="itemName">Name of the item to get the data for.</param>
	/// <returns>Inventory Object</returns>
	public InventoryObject GetItemData(string itemName) => itemData.FirstOrDefault(x => x.itemName == itemName);
}

[Serializable]
public class InventoryObject
{
	public string itemName;
	public string itemDescription;
	public string itemImagePath;
}
