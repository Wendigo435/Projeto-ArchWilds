using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Componentes")]
    public Image icon;
    public TMP_Text amountText;

    [HideInInspector] public int myIndex;
    private InventoryUI uiParent;
    private CanvasGroup canvasGroup;
    private Vector3 originalIconLocalPosition;

    void Awake()
    {
        uiParent = GetComponentInParent<InventoryUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void UpdateSlot(string itemName, int amount, Sprite itemSprite)
    {
        icon.sprite = itemSprite;
        icon.enabled = itemSprite != null;
        amountText.text = amount > 1 ? amount.ToString() : "";
    }

    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }

    public void ResetIconPosition()
    {
        icon.transform.localPosition = Vector3.zero;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (icon.sprite == null || !icon.enabled) return;
        originalIconLocalPosition = icon.transform.localPosition;
        icon.transform.SetAsLastSibling();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (icon.sprite == null || !icon.enabled) return;
        icon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        icon.transform.localPosition = originalIconLocalPosition;

        if (icon.sprite == null || !icon.enabled) return;

        GameObject hit = eventData.pointerCurrentRaycast.gameObject;
        if (hit != null && hit.TryGetComponent(out InventorySlot targetSlot))
        {
            uiParent.RequestSwap(myIndex, targetSlot.myIndex);
        }
        else if (hit == null)
        {
            uiParent.DropItem(myIndex);
        }
    }
}