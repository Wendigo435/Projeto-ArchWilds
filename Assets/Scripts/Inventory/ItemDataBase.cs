using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventario/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;

    public ItemData GetItemByID(int id)
    {
        if (allItems == null) return null;
        return allItems.Find(x => x.itemID == id);
    }
}