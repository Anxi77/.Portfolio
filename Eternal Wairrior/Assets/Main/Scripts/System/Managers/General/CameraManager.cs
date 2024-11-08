using UnityEngine;
using static StageManager;

public class CameraManager : SingletonManager<CameraManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] private GameObject virtualCameraPrefab;
    [SerializeField] private float townCameraSize = 8f;
    [SerializeField] private float gameCameraSize = 6f;
    private Cinemachine.CinemachineVirtualCamera virtualCamera;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public void Initialize()
    {
        try
        {
            Debug.Log("Initializing CameraManager...");
            IsInitialized = true;
            Debug.Log("CameraManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing CameraManager: {e.Message}");
            IsInitialized = false;
        }
    }

    public void SetupCamera(SceneType sceneType)
    {
        Debug.Log("Setting up camera...");

        // ���� ī�޶� ã��
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Looking for camera in scene...");
            mainCamera = FindObjectOfType<Camera>();
            if (mainCamera == null)
            {
                Debug.LogError("No camera found in scene at all!");
                return;
            }
        }

        // CinemachineBrain Ȯ�� �� �߰�
        var brain = mainCamera.GetComponent<Cinemachine.CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCamera.gameObject.AddComponent<Cinemachine.CinemachineBrain>();
        }

        if (virtualCameraPrefab == null)
        {
            Debug.LogError("Virtual Camera Prefab is not assigned!");
            return;
        }

        try
        {
            // ���� ���� ī�޶� ����
            if (virtualCamera != null)
            {
                Destroy(virtualCamera.gameObject);
            }

            // �� ī�޶� ���� (���� ī�޶��� �ڽ�����)
            GameObject camObj = Instantiate(virtualCameraPrefab, mainCamera.transform);
            camObj.transform.localPosition = Vector3.zero;
            virtualCamera = camObj.GetComponent<Cinemachine.CinemachineVirtualCamera>();

            if (virtualCamera == null)
            {
                Debug.LogError("Failed to get CinemachineVirtualCamera component!");
                return;
            }

            // �� Ÿ�Կ� ���� ī�޶� ����
            switch (sceneType)
            {
                case SceneType.Town:
                    virtualCamera.m_Lens.OrthographicSize = townCameraSize;
                    break;
                case SceneType.Game:
                    virtualCamera.m_Lens.OrthographicSize = gameCameraSize;
                    break;
            }

            // �÷��̾� ���� ����
            if (GameManager.Instance?.player != null)
            {
                virtualCamera.Follow = GameManager.Instance.player.transform;
            }
            else
            {
                Debug.LogWarning("Player not found for camera to follow!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting up camera: {e.Message}");
        }
    }

    public void ClearCamera()
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = null;
            Destroy(virtualCamera.gameObject);
            virtualCamera = null;
        }
    }
}