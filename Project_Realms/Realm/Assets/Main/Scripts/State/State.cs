using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    public void OnEnter();

    public void OnExit();

    public void OnUpdate();
}


public abstract class State<T> : IState where T : Component
{

    protected T target;

    public State(T target)
    {
        this.target = target;
    }

    public virtual void OnEnter()
    {

    }

    public virtual void OnExit()
    {

    }

    public virtual void OnUpdate()
    {

    }

}
