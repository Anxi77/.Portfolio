using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class StateHandler<T>
{
    public T target;
    
    public IState CurrentState {  get; protected set; }

    public StateHandler(T target)
    {
        this.target = target;
    }

    public event Action<IState> OnStateChanged;

    private bool hasStateChangedSubscribers => OnStateChanged != null;

    public bool HasSubscribers()
    {
        return hasStateChangedSubscribers;
    }

    public virtual void Initialize()
    {
        OnStateChanged?.Invoke(CurrentState);
    }

    public void TransitionTo(IState nextstate)
    {
        CurrentState.OnExit();
        CurrentState = nextstate;
        CurrentState.OnEnter();

        OnStateChanged?.Invoke(nextstate);
    }

    public virtual void HandleUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.OnUpdate();
        }
    }

}
