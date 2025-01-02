using System.Collections;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[RequireComponent(typeof(Collider))]
public class DebuffSKill : Skill
{
    [SerializeField]private Debuff DebuffPrefab;
    private Debuff DebuffParticle = null;
    private DeBuffSkillStat deBuffSkillStat;
    private bool isSkillActive = false;


    public override void Initialize(Unit owner)
    {
        base.Initialize(owner);
        deBuffSkillStat = (DeBuffSkillStat)skillStat;
        deBuffSkillStat.InitializeStats();
        this.transform.localScale = Vector3.one * deBuffSkillStat.GetStatValue<float>(SkillStatType.DeBuffAreaScale);
    }

    protected override void UseSkill()
    {

        Owner.Animator.SetTrigger("Attack");
        if (!isSkillActive)
        {
            DebuffParticle = PoolManager.Instance.Spawn<Debuff>(DebuffPrefab.gameObject, Owner.transform.position, Quaternion.Euler(90, 0, 0));
            DebuffParticle.transform.SetParent(Owner.transform);
            DebuffParticle.transform.localPosition = Vector3.zero;
            DebuffParticle.Initialize(Owner, deBuffSkillStat);
            isSkillActive = true;
        }
        else if (isSkillActive)
        {
            if (Owner is Player)
            {
                Owner.CharacterStats.AddModifier(StatType.Mana, CalcManaCost(deBuffSkillStat.GetStatValue<float>(SkillStatType.ManaCost)));
            }
            PoolManager.Instance.Despawn(DebuffParticle);
            DebuffParticle = null;
            isSkillActive = false;
        }
        Owner.Animator.SetTrigger("Idle");
    }

    private void OnDisable()
    {
        if (Owner is Player)
        {
            if (DebuffParticle != null)
            {
                PoolManager.Instance.Despawn(DebuffParticle);
                DebuffParticle = null;
                isSkillActive = false;
            }
        }
        else 
        {
            Destroy(DebuffParticle);
            isSkillActive = false;
        }
    }
}
