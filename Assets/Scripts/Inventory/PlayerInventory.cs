using UnityEngine;
using Mirror;

public class PlayerInventory : NetworkBehaviour
{
    [Header("Configurações")]
    public float PickDistance = 3f;
    public string itemTag = "Item";
    public KeyCode InvKey = KeyCode.Tab;
    public KeyCode PickKey = KeyCode.E;

    [Header("Componentes")]
    public readonly SyncList<Item> inventory = new SyncList<Item>();
    public ItemDatabase database;

    private InventoryUI UI;
    private Camera Cam;

    public override void OnStartLocalPlayer()
    {
        GameObject uiObj = GameObject.FindWithTag("InventoryUI");
        if (uiObj != null) UI = uiObj.GetComponent<InventoryUI>();
        if (UI != null) UI.BindPlayer(this);

        inventory.Callback += OnInventoryUpdated;
        Cam = Camera.main;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(InvKey))
        {
            if (UI == null)
            {
                GameObject uiObj = GameObject.FindWithTag("InventoryUI");
                if (uiObj != null) UI = uiObj.GetComponent<InventoryUI>();
            }
            if (UI != null) UI.ToggleUI();
        }

        if (Input.GetKeyDown(PickKey))
            TryPickup();
    }

    void OnInventoryUpdated(SyncList<Item>.Operation op, int index, Item oldItem, Item newItem)
    {
        Debug.Log($"Inventario atualizado! UI null: {UI == null}");
        if (isLocalPlayer && UI != null) UI.RefreshUI(inventory);
    }

    void TryPickup()
    {
        if (Cam == null) return;
        Ray ray = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, PickDistance))
        {
            if (hit.collider.CompareTag(itemTag))
            {
                if (hit.collider.TryGetComponent(out PickupItem pickup))
                    CmdPickupItem(pickup.gameObject);
            }
        }
    }

    [Command]
    public void CmdPickupItem(GameObject itemObject)
    {
        if (itemObject == null) return;
        if (!itemObject.TryGetComponent(out PickupItem pickup)) return;

        Item newItem = pickup.GetNetworkItem();
        ItemData dataSO = database.GetItemByID(newItem.itemID);

        bool found = false;
        if (dataSO != null && dataSO.stackable)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].itemID == newItem.itemID)
                {
                    Item temp = inventory[i];
                    temp.amount += newItem.amount;
                    inventory[i] = temp;
                    found = true;
                    break;
                }
            }
        }

        if (!found) inventory.Add(newItem);
        NetworkServer.Destroy(itemObject);
    }

    [Command]
    public void CmdSwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= inventory.Count || indexB < 0 || indexB >= inventory.Count) return;
        Item temp = inventory[indexA];
        inventory[indexA] = inventory[indexB];
        inventory[indexB] = temp;
    }

    [Command]
    public void CmdRemoveItem(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        Item networkItem = inventory[index];
        ItemData dataSO = database.GetItemByID(networkItem.itemID);

        if (dataSO != null && dataSO.worldPrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
            GameObject dropped = Instantiate(dataSO.worldPrefab, spawnPos, Quaternion.identity);

            if (dropped.TryGetComponent(out PickupItem pickup))
                pickup.amount = networkItem.amount;

            NetworkServer.Spawn(dropped);
        }

        inventory.RemoveAt(index);
    }
}