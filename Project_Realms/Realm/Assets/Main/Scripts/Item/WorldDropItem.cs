using UnityEngine;
using TMPro;

public class WorldDropItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Item item;
    [SerializeField] private TextMeshPro itemNameText;

    [Header("Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private float nameDisplayRadius = 5f;
    [SerializeField] private float floatingHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobAmount = 0.1f;

    private Vector3 startPosition;
    private bool isPlayerInRange;
    private Player player;
    private Camera mainCamera;
    private static WorldDropItem currentHoveredItem = null;

    [Header("Spawn Animation")]
    [SerializeField] private float spawnJumpHeight = 2f;
    [SerializeField] private float spawnRotationSpeed = 720f;
    [SerializeField] private float spawnAnimationTime = 0.5f;
    private float spawnStartTime;
    private bool isSpawning = true;
    private Vector3 originalRotation;
    private Transform textTransform;

    public float InteractionRadius => interactionRadius;
    public bool IsNameTextActive => itemNameText != null && itemNameText.gameObject.activeSelf;

    private void Awake()
    {
        if (itemNameText != null)
        {
            textTransform = itemNameText.transform;
            itemNameText.gameObject.SetActive(false);
            UpdateItemNameText();
        }
    }

    private void Start()
    {
        if (item == null) item = GetComponent<Item>();

        startPosition = transform.position;
        mainCamera = Camera.main;
        player = GameManager.Instance.player;

        spawnStartTime = Time.time;
        originalRotation = transform.eulerAngles;
        isSpawning = true;

        if (itemNameText != null)
        {
            textTransform = itemNameText.transform;
            itemNameText.gameObject.SetActive(false);
            UpdateItemNameText();

            itemNameText.alignment = TextAlignmentOptions.Center;
            itemNameText.fontSize = 5;
        }
    }

    private void Update()
    {
        if (isSpawning)
        {
            UpdateSpawnAnimation();
        }
        else
        {
            float newY = startPosition.y + floatingHeight + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            if (itemNameText != null && itemNameText.gameObject.activeSelf)
            {
                UpdateTextRotation();
            }
        }
    }

    public static void UpdateHoveredItem(WorldDropItem newHoveredItem)
    {
        if (currentHoveredItem == newHoveredItem) return;

        if (currentHoveredItem != null)
        {
            currentHoveredItem.HideTooltip();
        }

        currentHoveredItem = newHoveredItem;

        if (currentHoveredItem != null)
        {
            currentHoveredItem.ShowTooltip();
        }
    }

    public void TryPickupItem()
    {
        if (!IsNameTextActive) return;

        if (player == null) return;

        PlayerInventorySystem inventorySystem = player.InventorySystem;
        if (inventorySystem == null)
        {
            Debug.LogError("PlayerInventorySystem not found!");
            return;
        }

        GameObject newItemObj = new GameObject(item.ItemID.ToString());
        Item newItem = newItemObj.AddComponent<Item>();

        ItemInstanceData instanceData = ItemManager.Instance.GetItemInstanceData(item);
        if (instanceData == null)
        {
            Debug.LogError("Item instance data not found!");
            Destroy(newItemObj);
            return;
        }

        newItem.Initialize(item.ItemData, instanceData);
        newItemObj.transform.SetParent(player.transform);

        if (inventorySystem.AddItem(newItem))
        {
            ItemManager.Instance.RemoveItem(item);
            Destroy(gameObject);
        }
        else
        {
            Destroy(newItemObj);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, nameDisplayRadius);
    }

    private void UpdateSpawnAnimation()
    {
        float elapsedTime = Time.time - spawnStartTime;
        float progress = elapsedTime / spawnAnimationTime;

        if (progress >= 1f)
        {
            isSpawning = false;
            transform.eulerAngles = originalRotation;
            return;
        }

        float jumpProgress = Mathf.Sin(progress * Mathf.PI);
        float currentHeight = startPosition.y + (spawnJumpHeight * jumpProgress);

        float rotationAmount = spawnRotationSpeed * elapsedTime;

        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        transform.eulerAngles = new Vector3(
            originalRotation.x,
            originalRotation.y,
            originalRotation.z + rotationAmount
        );
    }

    private void ShowTooltip()
    {
        if (itemNameText != null && !itemNameText.gameObject.activeSelf)
        {
            itemNameText.gameObject.SetActive(true);
            UpdateTextRotation();
        }
    }

    private void HideTooltip()
    {
        if (itemNameText != null)
        {
            itemNameText.gameObject.SetActive(false);
        }
    }

    private void UpdateItemNameText()
    {
        if (itemNameText != null && item != null)
        {
            ItemInstanceData instanceData = ItemManager.Instance.GetItemInstanceData(item);
            if (instanceData != null)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(instanceData.NameColor);
                Color textColor = instanceData.NameColor;
                textColor.a = 1f;

                itemNameText.text = $"<color=#{colorHex}> {item.ItemID}</color>";
                itemNameText.color = textColor;
            }
        }
    }

    private void UpdateTextRotation()
    {
        if (itemNameText != null && mainCamera != null)
        {
            textTransform.LookAt(mainCamera.transform);
            textTransform.Rotate(0, 180, 0);
        }
    }
}