using System;
using System.Collections;
using TMPro;
using UnityEngine;
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
    [SerializeField]
    public PlayerUIPanel playerUIPanel;

    [Header("Boss Warning UI")]
    [SerializeField]
    private GameObject bossWarningPanel;

    [SerializeField]
    private float warningDuration = 3f;
    private Coroutine bossWarningCoroutine;

    private bool isPaused = false;

    [Header("Main Menu UI")]
    [SerializeField]
    private GameObject mainMenuPrefab;

    [SerializeField]
    private GameObject loadingScreenPrefab;

    public MainMenuPanel MainMenuPanel => mainMenuPanel;
    private MainMenuPanel mainMenuPanel;
    private LoadingScreen loadingScreen;

    [Header("Game Over UI")]
    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private TextMeshProUGUI gameOverTimerText;

    [Header("Stage UI")]
    [SerializeField]
    public StageTimeUI stageTimeUI;

    [Header("Inventory UI")]
    [SerializeField]
    private GameObject inventoryUIPrefab;
    private InventoryUI inventoryUI;

    [Header("Input Settings")]
    [SerializeField]
    private KeyCode inventoryToggleKey = KeyCode.I;

    private bool isInventoryAccessable = true;

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
            InitializeUIComponents();
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing UIManager: {e.Message}");
            IsInitialized = false;
        }
    }

    private void InitializeUIComponents()
    {
        mainCanvas = GetComponent<Canvas>();
        if (playerUIPanel != null)
        {
            playerUIPanel.Initialize();
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
                if (stageTimeUI != null)
                    stageTimeUI.gameObject.SetActive(false);
                break;
            case "GameScene":
            case "TestScene":
                StartCoroutine(SetupGameSceneUI());
                if (stageTimeUI != null)
                    stageTimeUI.gameObject.SetActive(true);
                break;
            case "BossStage":
                SetupBossStageUI();
                break;
        }
    }

    private void InitializeUI()
    {
        if (pausePanel)
            pausePanel.SetActive(false);
        if (levelupPanel)
            levelupPanel.gameObject.SetActive(false);
    }

    public void SetupMainMenuUI()
    {
        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas is not assigned!");
            return;
        }

        CleanupUI();
        InitializeMainMenuUI();
        ShowMainMenu();

        if (pausePanel)
        {
            pausePanel.SetActive(false);
        }
        if (levelupPanel)
        {
            levelupPanel.gameObject.SetActive(false);
        }
        if (playerUIPanel)
        {
            playerUIPanel.gameObject.SetActive(false);
        }
        if (bossWarningPanel)
        {
            bossWarningPanel.SetActive(false);
        }

        HideLoadingScreen();
        Time.timeScale = 1f;
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

    private void SetupBossStageUI() { }

    private void Update()
    {
        if (!IsInitialized)
            return;

        CheckPause();

        if (Input.GetKeyDown(inventoryToggleKey))
        {
            if (inventoryUI != null && inventoryUI.IsInitialized && isInventoryAccessable)
            {
                if (inventoryUI.gameObject.activeSelf)
                {
                    HideInventory();
                }
                else
                {
                    ShowInventory();
                }
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
        if (pausePanel)
            pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ShowLevelUpPanel()
    {
        if (levelupPanel != null && GameManager.Instance?.player != null)
        {
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
                skillList.skillListUpdate();
                Time.timeScale = 1f;
                levelupPanel.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No skill selected in level up panel");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnSkillSelected: {e.Message}");
            Time.timeScale = 1f;
            levelupPanel.gameObject.SetActive(false);
        }
    }

    public void ClearUI()
    {
        StopAllCoroutines();
        if (pausePanel)
            pausePanel.SetActive(false);
        if (levelupPanel)
            levelupPanel.gameObject.SetActive(false);
        if (stageTimeUI)
            stageTimeUI.gameObject.SetActive(false);
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

            var menuObj = Instantiate(mainMenuPrefab, mainCanvas.transform);
            mainMenuPanel = menuObj.GetComponent<MainMenuPanel>();

            if (mainMenuPanel == null)
            {
                Debug.LogError("Failed to get MainMenuPanel component!");
            }
        }
        else
        {
            Debug.LogError(
                $"MainMenuPanel already exists or prefab is null. Panel: {mainMenuPanel}, Prefab: {mainMenuPrefab}"
            );
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

            var loadingObj = Instantiate(loadingScreenPrefab, mainCanvas.transform);
            loadingScreen = loadingObj.GetComponent<LoadingScreen>();

            if (loadingScreen == null)
            {
                Debug.LogError("Failed to get LoadingScreen component!");
            }
            else
            {
                loadingScreen.gameObject.SetActive(false);
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
            loadingScreen.ResetProgress();
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
        if (!GameManager.Instance.HasSaveData())
            return;
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
        Application.Quit();
    }

    private void CleanupUI()
    {
        if (mainMenuPanel != null)
        {
            Destroy(mainMenuPanel.gameObject);
            mainMenuPanel = null;
        }
        if (loadingScreen != null)
        {
            Destroy(loadingScreen.gameObject);
            loadingScreen = null;
        }
        if (inventoryUI != null)
        {
            Destroy(inventoryUI.gameObject);
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

    public void SetInventoryAccessible(bool accessible)
    {
        isInventoryAccessable = accessible;
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

    public void InitializeInventoryUI()
    {
        if (inventoryUI != null)
        {
            Destroy(inventoryUI.gameObject);
            inventoryUI = null;
        }

        if (inventoryUIPrefab != null)
        {
            var inventoryObj = Instantiate(inventoryUIPrefab, mainCanvas.transform);
            inventoryUI = inventoryObj.GetComponent<InventoryUI>();

            inventoryUI.gameObject.SetActive(false);
            inventoryUI.Initialize();
            SetInventoryAccessible(false);
        }
        else
        {
            Debug.LogError("InventoryUI prefab is missing!");
        }
    }
}
