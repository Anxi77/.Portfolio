using UnityEngine;
using System.Collections;
using System.Buffers;
using static UnityEngine.UI.GridLayoutGroup;

public abstract class DefaultSkill : Skill
{
    protected WeaponHolder weaponHolder;
    protected ICharacterStats OwnerStats;
    protected bool isAttackInProgress = false;

    [Header("Weapon Settings")]
    [SerializeField] protected float attackAngle = 90f;
    [SerializeField] protected LayerMask targetLayer;

    public override void Initialize(Unit owner)
    {
        base.Initialize(owner);     
        OwnerStats = Owner.GetComponent<ICharacterStats>();        
        if (Owner.TryGetComponent<WeaponHolder>(out WeaponHolder WeaponHolder)) 
        {
            weaponHolder = WeaponHolder;
        }

    }

    public override bool TryUseSkill()
    {
        if (!CanUseSkill()) return false;
        if (isAttackInProgress) return false;

        Owner.Animator.SetFloat("AttackSpeed", 3f);

        if (animaClip != null)
        {
            Owner.AnimController.Clipchange(animaClip);
        }
        UseSkill();
        return true;
    }

    protected virtual bool CanUseSkill()
    {
        if (Owner == null || !Owner.IsAlive) return false;

        if (data.skillID == SkillID.BasicSwordAttack || data.skillID == SkillID.BasicBowAttack)
        {
            return Owner.Target != null;
        }

        return true;
    }

    protected virtual float GetAttackDelay()
    {
        if (data.skillID == SkillID.BasicSwordAttack || data.skillID == SkillID.BasicBowAttack || data.skillID == SkillID.MonsterSkill)
        {
            return 1f / GetAttackSpeed();
        }
        return 0.3f;
    }

    protected float GetDamage()
    {
        return OwnerStats?.GetStatValue(StatType.Damage) ?? 0f;
    }

    protected float GetAttackSpeed()
    {
        return OwnerStats?.GetStatValue(StatType.AttackSpeed) ?? 1f;
    }

    protected float GetAttackRange()
    {
        return OwnerStats?.GetStatValue(StatType.AttackRange) ?? 2f;
    }

    protected void OnAttackComplete()
    {
        isSkillInProgress = false;
        Owner.Animator.SetTrigger("Idle");
    }
}