using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerSkillState : State<Player>
{
    private bool isSkillComplete = false;
    private bool isSkillPlaying = false;
    public bool IsSkillPlaying => isSkillPlaying;

    public PlayerSkillState(Player target) : base(target)
    {
        this.target = target;
    }

    public override void OnEnter()
    {
        target.StopMoving();
        isSkillComplete = false;
        isSkillPlaying = true;

        if (target.Target != null)
        {
            target.skillController.TryUseSkillByKey(KeyCode.Mouse0);
        }
    }

    public override void OnExit()
    {
        target.ClearTarget();
        isSkillPlaying = false;
        isSkillComplete = false;
    }

    public override void OnUpdate()
    {
        AnimatorStateInfo stateInfo = target.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Attack"))
        {
            if (stateInfo.normalizedTime >= 0.97f)
            {
                isSkillComplete = true;
                isSkillPlaying = false;
            }
        }
        else
        {
            isSkillComplete = true;
            isSkillPlaying = false;
        }

        if (isSkillComplete)
        {
            if (target.Target != null)
            {
                float distanceToTarget = Vector3.Distance(target.transform.position, target.Target.transform.position);
                float attackRange = target.CharacterStats.GetStatValue(StatType.AttackRange);

                if (distanceToTarget > attackRange)
                {
                    target.PlayerHandler.TransitionTo(new PlayerMoveState(target));
                    return;
                }
            }

            target.PlayerHandler.TransitionTo(new PlayerIdleState(target));
            return;
        }
    }
}
