[System.Serializable]
public struct Item
{
    public int itemID;
    public int amount;

    public Item(int id, int qty)
    {
        itemID = id;
        amount = qty;
    }
}