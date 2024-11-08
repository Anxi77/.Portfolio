using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoopManager : SingletonManager<GameLoopManager>, IInitializable
{
    public enum GameState
    {
        MainMenu,
        Town,
        Stage,
        Paused,
        GameOver
    }

    public enum InitializationState
    {
        None,
        DataManagers,    // PlayerDataManager, ItemDataManager, SkillDataManager
        CoreManagers,    // GameManager, UIManager, PoolManager
        GameplayManagers,// PlayerUnitManager, MonsterManager, etc.
        Complete
    }

    private GameState currentState = GameState.MainMenu;
    public GameState CurrentState => currentState;
    public bool IsInitialized { get; private set; }

    private Dictionary<GameState, IGameStateHandler> stateHandlers;
    private InitializationState currentInitState = InitializationState.None;

    public void Initialize()
    {
        if (!IsInitialized)
        {
            StartInitialization();
        }
    }

    public void StartInitialization()
    {
        if (!IsInitialized)
        {
            StartCoroutine(InitializationSequence());
        }
    }

    private IEnumerator InitializationSequence()
    {
        Debug.Log("Starting manager initialization sequence...");

        // 1. Data Managers �ʱ�ȭ
        currentInitState = InitializationState.DataManagers;
        bool dataManagersInitialized = false;
        yield return StartCoroutine(InitializeDataManagers(success => dataManagersInitialized = success));

        if (!dataManagersInitialized)
        {
            Debug.LogError("Data Managers initialization failed");
            yield break;
        }

        // 2. Core Managers �ʱ�ȭ
        currentInitState = InitializationState.CoreManagers;
        bool coreManagersInitialized = false;
        yield return StartCoroutine(InitializeCoreManagers(success => coreManagersInitialized = success));

        if (!coreManagersInitialized)
        {
            Debug.LogError("Core Managers initialization failed");
            yield break;
        }

        // 3. Gameplay Managers �ʱ�ȭ
        currentInitState = InitializationState.GameplayManagers;
        bool gameplayManagersInitialized = false;
        yield return StartCoroutine(InitializeGameplayManagers(success => gameplayManagersInitialized = success));

        if (!gameplayManagersInitialized)
        {
            Debug.LogError("Gameplay Managers initialization failed");
            yield break;
        }

        // 4. State Handlers ���� �� �ʱ�ȭ
        if (!CreateStateHandlers())
        {
            Debug.LogError("State Handlers creation failed");
            yield break;
        }

        currentInitState = InitializationState.Complete;
        IsInitialized = true;

        Debug.Log("All managers initialized successfully");

        // StageManager�� ���� ���� �޴� �� �ε�
        if (StageManager.Instance != null)
        {
            Debug.Log("Loading main menu scene...");
            StageManager.Instance.LoadMainMenu();

            // �� �ε尡 �Ϸ�� ������ ���
            yield return new WaitForSeconds(0.5f);

            // ���� ����
            ChangeState(GameState.MainMenu);
            Debug.Log("Changed state to MainMenu");
        }
        else
        {
            Debug.LogError("StageManager is null, cannot load main menu!");
        }
    }

    private IEnumerator InitializeDataManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Data Managers...");

        // PlayerDataManager �ʱ�ȭ
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.Initialize();
            while (!PlayerDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerDataManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PlayerDataManager initialized");
        }

        // ItemDataManager �ʱ�ȭ
        if (ItemDataManager.Instance != null)
        {
            ItemDataManager.Instance.Initialize();
            while (!ItemDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(ItemDataManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("ItemDataManager initialized");
        }

        // SkillDataManager �ʱ�ȭ
        if (SkillDataManager.Instance != null)
        {
            SkillDataManager.Instance.Initialize();
            while (!SkillDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(SkillDataManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("SkillDataManager initialized");
        }

        Debug.Log("All Data Managers initialized");
        onComplete?.Invoke(true);
    }

    private bool CheckInitializationError(IInitializable manager)
    {
        // ���⼭ �ʱ�ȭ �� �߻��� �� �ִ� ���� ���¸� üũ
        // ��: Ÿ�Ӿƿ�, Ư�� ���� ���� ��
        return false; // ������ ������ false ��ȯ
    }

    private IEnumerator InitializeCoreManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Core Managers...");

        // PoolManager �ʱ�ȭ
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Initialize();
            while (!PoolManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PoolManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PoolManager initialized");
        }

        // GameManager �ʱ�ȭ
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Initialize();
            while (!GameManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(GameManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("GameManager initialized");
        }

        // CameraManager �ʱ�ȭ
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.Initialize();
            while (!CameraManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(CameraManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("CameraManager initialized");
        }

        // UIManager�� �������� �ʱ�ȭ
        if (UIManager.Instance != null)
        {
            // GameLoopManager�� �̹� �ʱ�ȭ�Ǿ� ������ ����
            IsInitialized = true;

            UIManager.Instance.Initialize();
            while (!UIManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(UIManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("UIManager initialized");
        }

        Debug.Log("All Core Managers initialized");
        onComplete?.Invoke(true);
    }

    private IEnumerator InitializeGameplayManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Gameplay Managers...");

        // SkillManager �ʱ�ȭ
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.Initialize();
            while (!SkillManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(SkillManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("SkillManager initialized");
        }

        // PlayerUnitManager �ʱ�ȭ
        if (PlayerUnitManager.Instance != null)
        {
            PlayerUnitManager.Instance.Initialize();
            while (!PlayerUnitManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerUnitManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PlayerUnitManager initialized");
        }

        // MonsterManager �ʱ�ȭ
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.Initialize();
            while (!MonsterManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(MonsterManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("MonsterManager initialized");
        }

        // StageTimeManager �ʱ�ȭ
        if (StageTimeManager.Instance != null)
        {
            StageTimeManager.Instance.Initialize();
            while (!StageTimeManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(StageTimeManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("StageTimeManager initialized");
        }

        // PlayerUIManager �ʱ�ȭ
        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.Initialize();
            while (!PlayerUIManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerUIManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PlayerUIManager initialized");
        }

        // ItemManager �ʱ�ȭ
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.Initialize();
            while (!ItemManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(ItemManager.Instance))
                {
                    onComplete?.Invoke(false);
                    yield break;
                }
                yield return null;
            }
            Debug.Log("ItemManager initialized");
        }

        Debug.Log("All Gameplay Managers initialized");
        onComplete?.Invoke(true);
    }

    private bool CreateStateHandlers()
    {
        Debug.Log("Creating state handlers...");
        stateHandlers = new Dictionary<GameState, IGameStateHandler>();

        try
        {
            // �� StateHandler �ν��Ͻ� ���� �� �ʱ�ȭ
            stateHandlers[GameState.MainMenu] = new MainMenuStateHandler();
            stateHandlers[GameState.Town] = new TownStateHandler();
            stateHandlers[GameState.Stage] = new StageStateHandler();
            stateHandlers[GameState.Paused] = new PausedStateHandler();
            stateHandlers[GameState.GameOver] = new GameOverStateHandler();

            Debug.Log("All state handlers created successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating state handlers: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState || !IsInitialized || stateHandlers == null)
            return;

        Debug.Log($"Changing state from {currentState} to {newState}");

        try
        {
            // ���� ���� ����
            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnExit();
            }

            // ���ο� ���·� ��ȯ
            currentState = newState;

            // ���ο� ���� ����
            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnEnter();
            }

            Debug.Log($"Successfully changed to state: {newState}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during state change: {e.Message}\n{e.StackTrace}");
        }
    }

    private void Update()
    {
        if (!IsInitialized || stateHandlers == null) return;

        try
        {
            stateHandlers[currentState]?.OnUpdate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in state update: {e.Message}");
        }
    }

    private void FixedUpdate()
    {
        if (!IsInitialized || stateHandlers == null) return;

        try
        {
            stateHandlers[currentState]?.OnFixedUpdate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in state fixed update: {e.Message}");
        }
    }

    public T GetCurrentHandler<T>() where T : class, IGameStateHandler
    {
        if (!IsInitialized || stateHandlers == null) return null;

        if (stateHandlers.TryGetValue(currentState, out var handler))
        {
            return handler as T;
        }
        return null;
    }

    private void OnDestroy()
    {
        IsInitialized = false;
        stateHandlers?.Clear();
        StopAllCoroutines();
    }
}

// ���� ���� �ڵ鷯 �������̽�
public interface IGameStateHandler
{
    void OnEnter();
    void OnUpdate();
    void OnFixedUpdate();
    void OnExit();
}
