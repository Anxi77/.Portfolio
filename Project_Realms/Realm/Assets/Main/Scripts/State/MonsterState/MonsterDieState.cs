using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDieState : State<Monster>
{
    public MonsterDieState(Monster target) : base(target)
    {
        this.target = target;
    }

    public override void OnEnter()
    {
        target.StopMoving();
        target.MonsterDie();
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {
        
    }
}
