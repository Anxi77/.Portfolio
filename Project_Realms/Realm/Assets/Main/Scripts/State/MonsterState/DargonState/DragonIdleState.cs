using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonIdleState : State<Dragon>
{
    private float patrolTime;

    public DragonIdleState(Dragon target) : base(target)
    {
        this.target = target;
    }

    public override void OnEnter()
    {
        patrolTime = target.UpdateTime;
        target.StopMoving();
        target.nextPatrol();
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
        patrolTime += Time.deltaTime;

        if (target.dragonHp < target.CharacterStats.GetStatValue(StatType.MaxHealth) / 2f)
        {
            target.DragonFormChange();
        }
        if (!target.IsAlive)
        {
            target.M_StateHandler.TransitionTo(new MonsterDieState(target));
            return;
        }
        var currentAnimatorState = target.Animator.GetCurrentAnimatorStateInfo(0);
        if (currentAnimatorState.IsName("Idle"))
        {
            Debug.Log($"{target} Idle 다음동작 대기중");
            if (target.CanAttack(target.Target))
            {
                Debug.Log($"{target} Attack 스테이트");
                target.M_StateHandler.TransitionTo(new DragonAttackState(target,target.CurrentSkill.data.skillID));
                return;
            }
            if (target.Target != null)
            {
                Debug.Log($"{target} Follow 스테이트");
                target.M_StateHandler.TransitionTo(new FollowState(target));
                return;
            }
            else if (target.FindPlayer(Mathf.Max(target.CharacterStats.GetStatValue(StatType.AttackRange), 20f)))
            {
                Debug.Log($"{target} Follow 스테이트");
                target.M_StateHandler.TransitionTo(new FollowState(target));
                return;
            }
            if (target.UpdateTime - patrolTime > 2f)
            {
                Debug.Log($"{target} move 스테이트");
                target.M_StateHandler.TransitionTo(new MonsterMoveState(target));
                return;
            }
        }
    }
}

