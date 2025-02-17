using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public partial class UIManager : SingletonManager<UIManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("UI Panels")]
    public Canvas mainCanvas;
    public GameObject pausePanel;
    public SkillLevelUpPanel levelupPanel;
    public PlayerSkillList skillList;

    [Header("Player Info")]
    [SerializeField] public PlayerUIPanel playerUIPanel;
    [SerializeField] public PlayerSkillList playerSkillList;

    [Header("UI Bars")]
    [SerializeField] private Slider hpBarImage;
    [SerializeField] private Slider expBarImage;

    [Header("Boss Warning UI")]
    [SerializeField] private GameObject bossWarningPanel;
    [SerializeField] private float warningDuration = 3f;
    private Coroutine bossWarningCoroutine;

    private bool isPaused = false;
    private Coroutine levelCheckCoroutine;

    [Header("Main Menu UI")]
    [SerializeField] private GameObject mainMenuPrefab;
    [SerializeField] private GameObject loadingScreenPrefab;

    public MainMenuPanel MainMenuPanel => mainMenuPanel;
    private MainMenuPanel mainMenuPanel;
    private LoadingScreen loadingScreen;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTimerText;

    [Header("Stage UI")]
    [SerializeField] public StageTimeUI stageTimeUI;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryUIPrefab;  // 프리팹만 참조
    private InventoryUI inventoryUI;  // 런타임에 생성된 인스턴스 참조

    [Header("Tooltips")]
    public GameObject tooltipPrefab;  // 툴팁 프리팹 참조 추가

    [Header("Input Settings")]
    [SerializeField] private KeyCode inventoryToggleKey = KeyCode.I;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (!GameLoopManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for GameLoopManager to initialize...");
            return;
        }

        try
        {
            Debug.Log("Initializing UIManager...");
            InitializeUIComponents();
            IsInitialized = true;
            Debug.Log("UIManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing UIManager: {e.Message}");
            IsInitialized = false;
        }
    }

    private void InitializeUIComponents()
    {
        if (playerUIPanel != null)
        {
            playerUIPanel.Initialize();
            Debug.Log("PlayerUIPanel initialized");
        }
        else
        {
            Debug.LogWarning("PlayerUIPanel reference is missing!");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        CleanupUI();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllCoroutines();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                SetupMainMenuUI();
                if (stageTimeUI != null) stageTimeUI.gameObject.SetActive(false);
                break;
            case "GameScene":
            case "TestScene":
                StartCoroutine(SetupGameSceneUI()); if (stageTimeUI != null) stageTimeUI.gameObject.SetActive(true);
                break;
            case "BossStage":
                SetupBossStageUI();
                break;
        }
    }

    private void InitializeUI()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (levelupPanel) levelupPanel.gameObject.SetActive(false);
    }

    public void SetupMainMenuUI()
    {
        Debug.Log("Starting SetupMainMenuUI");

        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas is not assigned!");
            return;
        }

        // 기존 UI 정리 전에 로그
        Debug.Log("Cleaning up existing UI");
        CleanupUI();

        // UI 초기화 전에 로그
        Debug.Log("Initializing main menu UI");
        InitializeMainMenuUI();

        // UI 표시 전에 로그
        Debug.Log("Showing main menu");
        ShowMainMenu();

        // 다른 UI 비활성화
        if (pausePanel)
        {
            pausePanel.SetActive(false);
            Debug.Log("Pause panel deactivated");
        }
        if (levelupPanel)
        {
            levelupPanel.gameObject.SetActive(false);
            Debug.Log("Level up panel deactivated");
        }
        if (playerUIPanel)
        {
            playerUIPanel.gameObject.SetActive(false);
            Debug.Log("Player UI deactivated");
        }
        if (bossWarningPanel)
        {
            bossWarningPanel.SetActive(false);
            Debug.Log("Boss warning panel deactivated");
        }

        HideLoadingScreen();
        Time.timeScale = 1f;

        Debug.Log("Main menu UI setup completed");
    }

    private IEnumerator SetupGameSceneUI()
    {
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }

        if (playerUIPanel != null && !playerUIPanel.gameObject.activeSelf)
        {
            playerUIPanel.gameObject.SetActive(true);
        }

        playerUIPanel?.InitializePlayerUI(GameManager.Instance.player);

        if (inventoryUI != null)
        {
            inventoryUI.gameObject.SetActive(true);
            if (!inventoryUI.IsInitialized)
            {
                inventoryUI.Initialize();
            }
            SetInventoryAccessible(true);
        }

        if (stageTimeUI != null)
        {
            stageTimeUI.gameObject.SetActive(true);
            if (!stageTimeUI.IsInitialized)
            {
                stageTimeUI.Initialize();
            }

            while (!StageTimeManager.Instance.IsInitialized)
            {
                yield return null;
            }
            StageTimeManager.Instance.StartStageTimer(600f);
        }
    }

    private void SetupBossStageUI()
    {
        // 보스 스테이지 UI 설정
    }

    private void Update()
    {
        if (!IsInitialized) return;

        CheckPause();

        // 인벤토리 토글 처리
        if (Input.GetKeyDown(inventoryToggleKey))
        {
            if (inventoryUI != null && inventoryUI.IsInitialized)
            {
                if (inventoryUI.gameObject.activeSelf)
                {
                    HideInventory();
                }
                else
                {
                    ShowInventory();
                }
                Debug.Log($"Inventory toggled: {inventoryUI.gameObject.activeSelf}");
            }
        }
    }

    private void CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pausePanel) pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ShowLevelUpPanel()
    {
        if (levelupPanel != null && GameManager.Instance?.player != null)
        {
            Debug.Log("Opening level up panel");
            levelupPanel.gameObject.SetActive(true);
            Time.timeScale = 0f;
            levelupPanel.LevelUpPanelOpen(GameManager.Instance.player.skills, OnSkillSelected);
        }
    }

    private void OnSkillSelected(Skill skill)
    {
        try
        {
            if (skill != null)
            {
                Debug.Log($"Skill selected: {skill.skillData.Name}");
                skillList.skillListUpdate();
                Time.timeScale = 1f;
                levelupPanel.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No skill selected in level up panel");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnSkillSelected: {e.Message}");
            Time.timeScale = 1f;
            levelupPanel.gameObject.SetActive(false);
        }
    }

    public void ClearUI()
    {
        StopAllCoroutines();
        if (pausePanel) pausePanel.SetActive(false);
        if (levelupPanel) levelupPanel.gameObject.SetActive(false);
        if (stageTimeUI) stageTimeUI.gameObject.SetActive(false);
        if (inventoryUI)
        {
            inventoryUI.gameObject.SetActive(false);
            SetInventoryAccessible(false);
        }
        playerUIPanel.Clear();
        Time.timeScale = 1f;
    }

    public void ShowBossWarning()
    {
        if (bossWarningCoroutine != null)
        {
            StopCoroutine(bossWarningCoroutine);
        }
        bossWarningCoroutine = StartCoroutine(ShowBossWarningCoroutine());
    }

    private IEnumerator ShowBossWarningCoroutine()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
            yield return new WaitForSeconds(warningDuration);
            bossWarningPanel.SetActive(false);
        }
        bossWarningCoroutine = null;
    }

    private void InitializeMainMenuUI()
    {
        if (mainMenuPanel == null && mainMenuPrefab != null)
        {
            if (mainCanvas == null)
            {
                Debug.LogError("Main Canvas is null during menu initialization!");
                return;
            }

            Debug.Log($"Creating main menu UI from prefab: {mainMenuPrefab.name}");
            var menuObj = Instantiate(mainMenuPrefab, mainCanvas.transform);
            mainMenuPanel = menuObj.GetComponent<MainMenuPanel>();

            if (mainMenuPanel == null)
            {
                Debug.LogError("Failed to get MainMenuPanel component!");
            }
            else
            {
                Debug.Log("MainMenuPanel component found successfully");
            }
        }
        else
        {
            Debug.Log($"MainMenuPanel already exists or prefab is null. Panel: {mainMenuPanel}, Prefab: {mainMenuPrefab}");
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel == null)
        {
            InitializeMainMenuUI();
        }
        mainMenuPanel?.gameObject.SetActive(true);
        mainMenuPanel?.UpdateButtons(GameManager.Instance.HasSaveData());
    }

    public void HideMainMenu()
    {
        mainMenuPanel?.gameObject.SetActive(false);
    }

    private void InitializeLoadingScreen()
    {
        if (loadingScreen == null && loadingScreenPrefab != null)
        {
            if (mainCanvas == null)
            {
                Debug.LogError("Main Canvas is null during loading screen initialization!");
                return;
            }

            Debug.Log($"Creating loading screen from prefab: {loadingScreenPrefab.name}");
            var loadingObj = Instantiate(loadingScreenPrefab, mainCanvas.transform);
            loadingScreen = loadingObj.GetComponent<LoadingScreen>();

            if (loadingScreen == null)
            {
                Debug.LogError("Failed to get LoadingScreen component!");
            }
            else
            {
                loadingScreen.gameObject.SetActive(false);
                Debug.Log("LoadingScreen component initialized successfully");
            }
        }
    }

    public void ShowLoadingScreen()
    {
        if (loadingScreen == null)
        {
            InitializeLoadingScreen();
        }

        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.ResetProgress();  // 진행률 초기화
            Debug.Log("Loading screen shown");
        }
        else
        {
            Debug.LogError("Failed to show loading screen - loading screen is null");
        }
    }

    public void HideLoadingScreen()
    {
        loadingScreen?.gameObject.SetActive(false);
    }

    public void UpdateLoadingProgress(float progress)
    {
        loadingScreen?.UpdateProgress(progress);
    }

    public void OnStartNewGame()
    {
        GameManager.Instance.InitializeNewGame();
        StartCoroutine(LoadTownScene());
    }

    public void OnLoadGame()
    {
        if (!GameManager.Instance.HasSaveData()) return;
        GameManager.Instance.LoadGameData();
        StartCoroutine(LoadTownScene());
    }

    private IEnumerator LoadTownScene()
    {
        StageManager.Instance.LoadTownScene();
        yield break;
    }

    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CleanupUI()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Attempting to cleanup UI in edit mode - operation skipped");
            return;
        }
#endif

        if (mainMenuPanel != null)
        {
            DestroyImmediate(mainMenuPanel.gameObject);
            mainMenuPanel = null;
        }
        if (loadingScreen != null)
        {
            DestroyImmediate(loadingScreen.gameObject);
            loadingScreen = null;
        }
        if (inventoryUI != null)
        {
            DestroyImmediate(inventoryUI.gameObject);
            inventoryUI = null;
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void HideGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void UpdateGameOverTimer(float time)
    {
        if (gameOverTimerText != null)
        {
            gameOverTimerText.text = $"타운으로 돌아가기: {time:F1}초";
        }
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void InitializePlayerUI(Player player)
    {
        if (playerUIPanel != null)
        {
            playerUIPanel.InitializePlayerUI(player);
            GameManager.Instance.StartLevelCheck();
        }
    }

    public bool IsMainMenuActive()
    {
        return mainMenuPanel != null && mainMenuPanel.gameObject.activeSelf;
    }

    public void SetupGameUI()
    {
        if (playerUIPanel != null)
        {
            playerUIPanel.Initialize();
        }
        else
        {
            Debug.LogError("PlayerUIPanel reference is missing!");
        }
    }

    public bool IsGameUIReady()
    {
        return playerUIPanel != null && playerUIPanel.IsUIReady;
    }

    public bool IsLoadingScreenVisible()
    {
        return loadingScreen != null && loadingScreen.gameObject.activeSelf;
    }

    public void UpdatePlayerUI(Player player)
    {
        if (playerUIPanel != null)
        {
            playerUIPanel.InitializePlayerUI(player);
        }
    }

    public void ClearPlayerUI()
    {
        playerUIPanel?.Clear();
    }

    public bool IsPlayerUIReady()
    {
        return playerUIPanel != null &&
               playerUIPanel.gameObject.activeSelf &&
               playerUIPanel.IsUIReady;
    }

    public void SetInventoryAccessible(bool accessible)
    {
        inventoryUI?.SetInventoryAccessible(accessible);
    }

    public void ShowInventory()
    {
        if (inventoryUI != null && inventoryUI.IsInitialized)
        {
            inventoryUI.gameObject.SetActive(true);
            inventoryUI.UpdateUI();
        }
    }

    public void HideInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.gameObject.SetActive(false);
        }
    }

    public void UpdateInventoryUI()
    {
        inventoryUI?.UpdateUI();
    }

    public ItemTooltip CreateTooltip(Vector2 position)
    {
        if (tooltipPrefab == null)
        {
            Debug.LogError("Tooltip prefab not assigned in UIManager!");
            return null;
        }

        var tooltip = PoolManager.Instance.Spawn<ItemTooltip>(tooltipPrefab, position, Quaternion.identity);
        if (tooltip != null)
        {
            tooltip.transform.SetParent(mainCanvas.transform, false);
        }
        return tooltip;
    }

    public void InitializeInventoryUI()
    {
        // 이미 존재하는 인벤토리 UI가 있다면 제거
        if (inventoryUI != null)
        {
            Destroy(inventoryUI.gameObject);
            inventoryUI = null;
        }

        // 프리팹으로부터 새로운 인스턴스 생성
        if (inventoryUIPrefab != null)
        {
            var inventoryObj = Instantiate(inventoryUIPrefab, mainCanvas.transform);
            inventoryUI = inventoryObj.GetComponent<InventoryUI>();
            Debug.Log("Created new InventoryUI instance");

            inventoryUI.gameObject.SetActive(false);
            inventoryUI.Initialize();
            SetInventoryAccessible(false);
            Debug.Log("InventoryUI initialized");
        }
        else
        {
            Debug.LogError("InventoryUI prefab is missing!");
        }
    }
}
