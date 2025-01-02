using UnityEngine;
using UnityEngine.UI;


public class DragItemHolder : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Transform dragIconTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    public bool IsDragging { get; private set; }
    public Item DraggedItem { get; private set; }

    private Slot targetSlot;
    private Slot sourceSlot;

    public Slot SourceSlot => sourceSlot;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideIcon();
    }

    private void HideIcon()
    {
        itemIconImage.enabled = false;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void ShowIcon()
    {
        itemIconImage.enabled = true;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void StartDrag(Slot source)
    {
        if (IsDragging || source.IsEmpty()) return;

        IsDragging = true;
        sourceSlot = source;
        DraggedItem = sourceSlot.Item;
        sourceSlot.ClearSlot();

        SetTargetSlot(sourceSlot);

        dragIconTransform.position = Input.mousePosition;
        itemIconImage.sprite = DraggedItem.Icon;
        ShowIcon();
    }

    public void SetTargetSlot(Slot newTarget)
    {
        if (!IsDragging) return;

        // 이전 타겟 슬롯 초기화
        if (targetSlot != null)
            targetSlot.ClearSlot();

        targetSlot = newTarget;

        // 새 타겟 슬롯에 미리보기 표시
        if (targetSlot.CanAcceptItem(DraggedItem))
            targetSlot.PlaceholdItem(DraggedItem);
    }

    public void DropItem()
    {
        if (!IsDragging) return;

        // 현재 타겟 슬롯에 아이템 배치
        if (targetSlot != null && targetSlot.CanAcceptItem(DraggedItem))
            targetSlot.PlaceItem(DraggedItem);
        else // 유효하지 않은 위치에 드롭시 원래 위치로 복귀
            sourceSlot.PlaceItem(DraggedItem);

        // 드래그 상태 초기화
        IsDragging = false;
        DraggedItem = null;
        HideIcon();
        targetSlot = null;
        sourceSlot = null;
    }

    private void Update()
    {
        if (IsDragging)
            dragIconTransform.position = Input.mousePosition;
    }

    public void RemoveTargetSlot(Slot slot)
    {
        if (slot != targetSlot) return;
        SetTargetSlot(sourceSlot);
    }
}
