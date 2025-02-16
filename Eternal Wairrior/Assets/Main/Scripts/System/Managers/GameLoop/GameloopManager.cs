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

    private GameState currentState = GameState.MainMenu;
    public GameState CurrentState => currentState;
    public bool IsInitialized { get; private set; }

    private Dictionary<GameState, IGameStateHandler> stateHandlers;

    private bool isStateTransitioning = false;

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
        yield return StartCoroutine(InitializeDataManagers(success =>
        {
            if (!success)
            {
                Debug.LogError("Failed to initialize Data Managers");
            }
        }));

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(InitializeCoreManagers(success =>
        {
            if (!success)
            {
                Debug.LogError("Failed to initialize Core Managers");
            }
        }));

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(InitializeGameplayManagers());

        yield return new WaitForSeconds(0.1f);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Initialize();
        }

        if (CreateStateHandlers())
        {
            Debug.Log("State handlers initialized successfully");
            ChangeState(GameState.MainMenu);
        }
        else
        {
            Debug.LogError("Failed to initialize state handlers");
        }
    }

    private IEnumerator InitializeDataManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Data Managers...");
        bool success = true;

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.Initialize();
            while (!PlayerDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerDataManager.Instance))
                {
                    success = false;
                    break;
                }
                yield return null;
            }
            Debug.Log("PlayerDataManager initialized");
        }

        if (success && ItemDataManager.Instance != null)
        {
            ItemDataManager.Instance.Initialize();
            while (!ItemDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(ItemDataManager.Instance))
                {
                    success = false;
                    break;
                }
                yield return null;
            }
            Debug.Log("ItemDataManager initialized");
        }

        if (success && SkillDataManager.Instance != null)
        {
            SkillDataManager.Instance.Initialize();
            while (!SkillDataManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(SkillDataManager.Instance))
                {
                    success = false;
                    break;
                }
                yield return null;
            }
            Debug.Log("SkillDataManager initialized");
        }

        Debug.Log($"Data Managers initialization {(success ? "completed" : "failed")}");
        onComplete?.Invoke(success);
    }

    private bool CheckInitializationError(IInitializable manager)
    {
        // 여기서 초기화 중 발생할 수 있는 오류 상태를 체크
        // 예: 타임아웃, 특정 조건 실패 등
        return false; // 오류가 없으면 false 반환
    }

    private IEnumerator InitializeCoreManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Core Managers...");

        // PoolManager 초기화
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

        // GameManager 초기화
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

        // CameraManager 초기화
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

        // UIManager는 마지막에 초기화
        if (UIManager.Instance != null)
        {
            // GameLoopManager가 이미 초기화되어 있음을 보장
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

    private IEnumerator InitializeGameplayManagers()
    {
        Debug.Log("Initializing Gameplay Managers...");

        // ItemManager 초기화 전에 ItemDataManager가 완전히 초기화될 때까지 대기
        while (ItemDataManager.Instance == null || !ItemDataManager.Instance.IsInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // ItemManager 초기화
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.Initialize();
            while (!ItemManager.Instance.IsInitialized)
            {
                yield return null;
            }
            Debug.Log("ItemManager initialized");
        }

        // SkillManager 초기화
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.Initialize();
            while (!SkillManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(SkillManager.Instance))
                {
                    yield break;
                }
                yield return null;
            }
            Debug.Log("SkillManager initialized");
        }

        // PlayerUnitManager 초기화
        if (PlayerUnitManager.Instance != null)
        {
            PlayerUnitManager.Instance.Initialize();
            while (!PlayerUnitManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(PlayerUnitManager.Instance))
                {
                    yield break;
                }
                yield return null;
            }
            Debug.Log("PlayerUnitManager initialized");
        }

        // MonsterManager 초기화
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.Initialize();
            while (!MonsterManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(MonsterManager.Instance))
                {
                    yield break;
                }
                yield return null;
            }
            Debug.Log("MonsterManager initialized");
        }

        // StageTimeManager 초기화
        if (StageTimeManager.Instance != null)
        {
            StageTimeManager.Instance.Initialize();
            while (!StageTimeManager.Instance.IsInitialized)
            {
                if (CheckInitializationError(StageTimeManager.Instance))
                {
                    yield break;
                }
                yield return null;
            }
            Debug.Log("StageTimeManager initialized");
        }

        Debug.Log("All Gameplay Managers initialized");
    }

    private bool CreateStateHandlers()
    {
        Debug.Log("Creating state handlers...");
        stateHandlers = new Dictionary<GameState, IGameStateHandler>();

        try
        {
            // 각 StateHandler 인스턴스 생성 및 초기화
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
            // 이미 상태 전환이 진행 중인지 확인
            if (isStateTransitioning)
            {
                Debug.Log("State transition already in progress, skipping");
                return;
            }

            // 상태 전환 시작
            isStateTransitioning = true;

            // 현재 상태 종료
            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnExit();
            }

            // 새로운 상태로 전환
            currentState = newState;

            // 새로운 상태 시작
            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnEnter();
            }

            // 상태 전환 완료
            isStateTransitioning = false;
            Debug.Log($"Successfully changed to state: {newState}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during state change: {e.Message}\n{e.StackTrace}");
            isStateTransitioning = false;
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

// 게임 상태 핸들러 인터페이스
public interface IGameStateHandler
{
    void OnEnter();
    void OnUpdate();
    void OnFixedUpdate();
    void OnExit();
}
