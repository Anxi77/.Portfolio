using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieState : State<Player>
{
    public PlayerDieState(Player target) : base(target)
    {
        this.target = target;
    }

    public override void OnEnter()
    {
        target.StopMoving();
        target.PlayerDie();
    }
    public override void OnExit()
    {
        
    }
    public override void OnUpdate()
    {
        
    }

}
