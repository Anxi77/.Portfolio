using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class TooltipWindow : MonoBehaviour
{
    private static TooltipWindow instance;
    public static TooltipWindow Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TooltipWindow>();
            }
            return instance;
        }
    }

    [SerializeField] private RectTransform windowRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private TextMeshProUGUI itemDescText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemRarityText;

    private Camera mainCamera;
    private Canvas parentCanvas;
    private Vector2 offset = new Vector2(5f, 5f);
    private RectTransform canvasRectTransform;
    private bool isShowing = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Canvas highestCanvas = canvases
            .Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay)
            .OrderByDescending(c => c.sortingOrder)
            .FirstOrDefault();

        if (highestCanvas != null)
        {
            transform.SetParent(highestCanvas.transform, false);
            transform.SetAsLastSibling();
        }

        mainCamera = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRectTransform = parentCanvas.GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        InitializeTooltip();
    }
    private void InitializeTooltip()
    {
        isShowing = false;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void ShowTooltip(string title, string rarity, string type, string description, string content, Sprite icon, float xPosition)
    {
        if (isShowing) return;

        isShowing = true;
        itemNameText.text = title;
        itemRarityText.text = rarity;
        itemTypeText.text = type;
        itemDescText.text = description;
        tooltipText.text = content;

        if (itemIconImage != null)
        {
            itemIconImage.sprite = icon;
            itemIconImage.gameObject.SetActive(icon != null);
        }

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(windowRect);
        Canvas.ForceUpdateCanvases();

        UpdatePosition();
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        if (!isShowing) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 size = windowRect.rect.size;
        Vector2 position = mousePos + offset;

        if (position.x + size.x > Screen.width)
        {
            position.x = mousePos.x - size.x - offset.x * 3;
        }

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 screenPoint = position;
            Vector2 worldPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPoint,
                parentCanvas.worldCamera,
                out worldPos
            );
            windowRect.localPosition = worldPos;
        }
        else
        {
            windowRect.position = position;
        }
    }

    public void HideTooltip()
    {
        if (!isShowing) return;

        isShowing = false;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}