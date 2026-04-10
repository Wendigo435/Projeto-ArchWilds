using UnityEngine;
using Mirror;

public class PickupItem : NetworkBehaviour
{
    public int itemID;
    public int amount = 1;

    public Item GetNetworkItem()
    {
        return new Item(itemID, amount);
    }
}