using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTakeDamageState : State<Player>
{
    public PlayerTakeDamageState(Player target) : base(target)
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
        if(!target.IsAlive) 
        {
            target.PlayerHandler.TransitionTo(new PlayerDieState(target));
        }
        else if (target.IsAlive)
        {
            target.Animator.SetTrigger("Idle");
            target.PlayerHandler.TransitionTo(new PlayerIdleState(target));      
        }
    }



}
