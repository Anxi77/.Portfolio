using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MonsterAttackState : State<Monster>
{

    public MonsterAttackState(Monster target) : base(target)
    {
        this.target = target;
    }

    public override void OnEnter()
    {
        target.StopMoving();
        if (target.CurrentSkill == null) 
        {
            target.GetSkill(SkillID.MonsterSkill);
        }
        target.CurrentSkill.TryUseSkill();
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {
        var currentAnimatorState = target.Animator.GetCurrentAnimatorStateInfo(0);
        if (currentAnimatorState.normalizedTime >= 1f)
        {
            if (target.wasAttacked)
            {
                target.M_StateHandler.TransitionTo(new MonsterTakeDamageState(target));
            }
            if (!target.wasAttacked || !target.CanAttack(target.Target))
            {
                target.Animator.SetTrigger("Idle");
                target.M_StateHandler.TransitionTo(new MonsterIdleState(target));
            }
            target.Animator.SetTrigger("Idle");
        }
    }

}
