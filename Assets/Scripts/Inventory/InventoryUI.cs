using UnityEngine;
using Mirror;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject inventoryPanel;

    [Header("Slots")]
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public List<InventorySlot> hotbarSlots = new List<InventorySlot>();

    [Header("Database")]
    public ItemDatabase database;

    public static bool isOpen;
    private PlayerInventory localPlayer;

    void Awake()
    {
        inventoryPanel.SetActive(false);
        isOpen = false;
    }

    public void BindPlayer(PlayerInventory player)
    {
        localPlayer = player;
        RefreshUI(player.inventory);
    }

    public void ToggleUI()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (!isOpen)
        {
            foreach (var slot in inventorySlots)
                if (slot != null) slot.ResetIconPosition();
            foreach (var slot in hotbarSlots)
                if (slot != null) slot.ResetIconPosition();
        }
    }

    public void RefreshUI(SyncList<Item> inventory)
    {
        Debug.Log($"RefreshUI chamado! inventory.Count: {inventory.Count} database null: {database == null}");

        // Atualiza hotbar (primeiros 9 slots)
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (hotbarSlots[i] == null) continue;
            hotbarSlots[i].myIndex = i;
            hotbarSlots[i].ClearSlot();

            if (i < inventory.Count)
            {
                ItemData data = database.GetItemByID(inventory[i].itemID);
                if (data != null)
                    hotbarSlots[i].UpdateSlot(data.itemName, inventory[i].amount, data.icon);
            }
        }

        // Atualiza inventário (slots após a hotbar)
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == null) continue;
            int inventoryIndex = i + hotbarSlots.Count;
            inventorySlots[i].myIndex = inventoryIndex;
            inventorySlots[i].ClearSlot();

            if (inventoryIndex < inventory.Count)
            {
                ItemData data = database.GetItemByID(inventory[inventoryIndex].itemID);
                if (data != null)
                    inventorySlots[i].UpdateSlot(data.itemName, inventory[inventoryIndex].amount, data.icon);
            }
        }
    }

    public void RequestSwap(int fromIndex, int toIndex)
    {
        if (localPlayer != null)
            localPlayer.CmdSwapItems(fromIndex, toIndex);
    }

    public void DropItem(int slotIndex)
    {
        if (localPlayer != null)
            localPlayer.CmdRemoveItem(slotIndex);
    }
}