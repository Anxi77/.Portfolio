using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MonsterMoveState : State<Monster>
{
    public MonsterMoveState(Monster target) : base(target)
    {
        this.target = target;
    }
    private float MoveStateTime = 0;

    public override void OnEnter()
    {
        target.StopMoving();
        MoveStateTime = target.UpdateTime;
        if (!target.IsMoving)
        {
            target.Animator.SetBool("Move", true);
            target.MoveTo(target.currentPatrolPoint);
        }
    }

    public override void OnExit()
    {

        target.Animator.SetBool("Move", false);
    }

    public override void OnUpdate()
    {
        MoveStateTime += Time.deltaTime;


        if (target is Dragon dragon)
        {
            if (!target.IsAlive)
            {
                target.M_StateHandler.TransitionTo(new MonsterDieState(dragon));
                return;
            }
            if (target.FindPlayer(15f))
            {
                target.M_StateHandler.TransitionTo(new FollowState(dragon));
                return;
            }
            if (target.UpdateTime-MoveStateTime > 5f)
            {
                target.Animator.SetTrigger("Idle");
                target.M_StateHandler.TransitionTo(new DragonIdleState(dragon));
                return;
            }

        }
        else
        {
            if (!target.IsAlive)
            {
                target.M_StateHandler.TransitionTo(new MonsterDieState(target));
                return;
            }
            if (target.wasAttacked)
            {
                target.M_StateHandler.TransitionTo(new MonsterTakeDamageState(target));
                return;
            }
            if (target.FindPlayer(6f))
            {
                target.M_StateHandler.TransitionTo(new FollowState(target));
                return;
            }
            if (target.UpdateTime-MoveStateTime > 5f)
            {
                target.Animator.SetTrigger("Idle");
                target.M_StateHandler.TransitionTo(new MonsterIdleState(target));
                return;
            }
        }
    }
}