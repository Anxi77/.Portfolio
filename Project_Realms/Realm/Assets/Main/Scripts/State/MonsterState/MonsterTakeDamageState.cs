using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTakeDamageState : State<Monster>
{
    public MonsterTakeDamageState(Monster target) : base(target)
    {
        this.target = target;
    }

    public override void OnEnter()
    {
        target.Animator.SetTrigger("TakeDamage");
    }

    public override void OnExit()
    {
        target.wasAttacked = false;
    }

    public override void OnUpdate()
    {
        target.FindPlayer(20f);
        if (!target.IsAlive)
        {
            target.M_StateHandler.TransitionTo(new MonsterDieState(target));
        }
        else if (target.IsAlive)
        {
            target.Animator.SetTrigger("Idle");
            target.M_StateHandler.TransitionTo(new MonsterIdleState(target));
        }
    }

}
