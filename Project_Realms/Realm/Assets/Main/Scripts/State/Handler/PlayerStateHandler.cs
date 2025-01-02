using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateHandler : StateHandler<Player>
{
    public PlayerStateHandler(Player target) : base(target)
    {
        this.target = target;
    }

    public override void Initialize()
    {
        CurrentState = new PlayerIdleState(target);
        CurrentState.OnEnter();
        base.Initialize();
    }
}
