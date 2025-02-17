using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        if (manager == null)
        {
            Debug.LogError($"Manager is null: {manager.GetType().Name}");
            return true;
        }
        if (!manager.IsInitialized)
        {
            Debug.LogError($"Manager is not initialized: {manager.GetType().Name}");
            return true;
        }
        return false;
    }

    private IEnumerator InitializeCoreManagers(System.Action<bool> onComplete)
    {
        Debug.Log("Initializing Core Managers...");

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

        if (UIManager.Instance != null)
        {
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

        while (ItemDataManager.Instance == null || !ItemDataManager.Instance.IsInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.Initialize();
            while (!ItemManager.Instance.IsInitialized)
            {
                yield return null;
            }
            Debug.Log("ItemManager initialized");
        }

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
            stateHandlers[GameState.MainMenu] = new MainMenuStateHandler();
            stateHandlers[GameState.Town] = new TownStateHandler();
            stateHandlers[GameState.Stage] = new StageStateHandler();
            stateHandlers[GameState.Paused] = new PausedStateHandler();
            stateHandlers[GameState.GameOver] = new GameOverStateHandler();

            Debug.Log("All state handlers created successfully");
            return true;
        }
        catch (Exception e)
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
            if (isStateTransitioning)
            {
                Debug.Log("State transition already in progress, skipping");
                return;
            }

            isStateTransitioning = true;

            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnExit();
            }

            currentState = newState;

            if (stateHandlers.ContainsKey(currentState))
            {
                stateHandlers[currentState].OnEnter();
            }

            isStateTransitioning = false;
            Debug.Log($"Successfully changed to state: {newState}");
        }
        catch (Exception e)
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
        catch (Exception e)
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
        catch (Exception e)
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

public interface IGameStateHandler
{
    void OnEnter();
    void OnUpdate();
    void OnFixedUpdate();
    void OnExit();
}
