using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon : Monster
{
    public UnitState dragonState { get; set; } = UnitState.Ground;

    private bool FlyState = false;

    public float dragonHp => this.characterStats.GetStatValue(StatType.Health);

    public override void Initialize()
    {
        base.Initialize();

        AnimController = gameObject.GetComponent<AnimatorController>();
        AnimController.Initialize(this);
     
        AnimController.DragonAnimatorChange(UnitState.Ground);
        GetSkill(SkillID.DragonBite);
        dragonState = UnitState.Ground;
        FlyState = false;
    }


    public void DragonFormChange()
    {
        if (FlyState) return;

        if (dragonState == UnitState.Ground)
        {
            dragonState = UnitState.Fly;
        }
        else
        {
            dragonState = UnitState.Ground;
        }

        GetSkill(SkillID.DragonNova).TryUseSkill();
        AnimController.DragonAnimatorChange(dragonState);
        GetSkill(SkillID.DragonBreath);

        FlyState = true;

    }

    public override IEnumerator DieRoutine()
    {
        Animator.SetTrigger("Die");

        while (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            Animator.SetTrigger("Die");
            yield return null;
        }

        while (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.75f)
        {
            yield return null;
        }

        DropExpParticle();

        List<Item> droppedItems = ItemManager.Instance.GenerateRandomItems(monsterType, transform.position);

        ParticleSystem mdp = PoolManager.Instance.Spawn<ParticleSystem>(monsterDieParticle.gameObject, transform.position, Quaternion.identity);
        mdp.Play();
        MonsterManager.Instance.currentMonsters.Remove(this);
        PoolManager.Instance.Despawn(mdp, 1f);
        Destroy(this.gameObject);
    }

}
