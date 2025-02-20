using System;
using UnityEngine;

public class StageTimeManager : SingletonManager<StageTimeManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    private float stageTimer;
    private float stageDuration;
    private bool isTimerRunning;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public void Initialize()
    {
        try
        {
            ResetTimer();
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing StageTimeManager: {e.Message}");
            IsInitialized = false;
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            stageTimer += Time.deltaTime;
        }
    }

    public void StartStageTimer(float duration)
    {
        stageDuration = duration;
        stageTimer = 0f;
        isTimerRunning = true;
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void ResumeTimer()
    {
        isTimerRunning = true;
    }

    public void ResetTimer()
    {
        stageTimer = 0f;
        stageDuration = 0f;
        isTimerRunning = false;
    }

    public bool IsStageTimeUp()
    {
        return stageTimer >= stageDuration;
    }

    public float GetElapsedTime()
    {
        return stageTimer;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, stageDuration - stageTimer);
    }

    public float GetTimeProgress()
    {
        return stageDuration > 0f ? stageTimer / stageDuration : 0f;
    }
}
