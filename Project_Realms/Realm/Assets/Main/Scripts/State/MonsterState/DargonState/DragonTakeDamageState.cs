using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class DragonTakeDamageState : State<Dragon>
//{
//    public DragonTakeDamageState(Dragon target) : base(target)
//    {
//        this.target = target;
//    }

//    public override void OnEnter()
//    {
//        target.Animator.SetTrigger("TakeDamage");
//        if (target.dragonHp < target.CharacterStats.GetStatValue(StatType.MaxHealth) / 2f)
//        {
//            target.DragonFormChange();
//        }
//    }

//    public override void OnExit()
//    {
//        target.wasAttacked = false;
//    }

//    public override void OnUpdate()
//    {
//        target.FindPlayer(20f);
//        if (!target.IsAlive)
//        {
//            target.M_StateHandler.TransitionTo(new MonsterDieState(target));
//        }
//        else if (target.IsAlive)
//        {
//            target.Animator.SetTrigger("Idle");
//            target.M_StateHandler.TransitionTo(new DragonIdleState(target));
//        }
//    }
//}
