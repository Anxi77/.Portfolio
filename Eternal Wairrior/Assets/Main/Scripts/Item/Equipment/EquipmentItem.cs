using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class EquipmentItem : Item, ISkillModifier
{
    protected EquipmentSlot equipmentSlot;
    protected List<ISkillInteractionEffect> skillEffects = new();
    protected new ItemData itemData;

    protected EquipmentItem(ItemData itemData)
    {
        this.itemData = itemData;
    }

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        ValidateItemType(data.Type);
        InitializeSkillEffects(data);
    }

    protected virtual void InitializeSkillEffects(ItemData data)
    {
        skillEffects = SkillEffectFactory.CreateEffects(data);
    }

    protected void OnEquip(Player player)
    {
        foreach (var skill in player.skills)
        {
            foreach (var effect in skillEffects)
            {
                skill.ApplyItemEffect(effect);
            }
        }
    }

    protected void OnUnequip(Player player)
    {
        foreach (var skill in player.skills)
        {
            foreach (var effect in skillEffects)
            {
                skill.RemoveItemEffect(effect);
            }
        }
    }

    protected abstract void ValidateItemType(ItemType type);

    public virtual float ModifySkillDamage(float baseDamage, SkillType skillType, ElementType elementType)
    {
        float multiplier = 1f;
        foreach (var effect in skillEffects.OfType<SkillStatAmplifierEffect>())
        {
            if (effect.CanApplyTo(skillType, elementType))
            {
                multiplier *= effect.damageMultiplier;
            }
        }
        return baseDamage * multiplier;
    }

    public virtual float ModifySkillCooldown(float baseCooldown, SkillType skillType)
    {
        float reduction = 0f;
        foreach (var effect in skillEffects.OfType<SkillStatAmplifierEffect>())
        {
            if (effect.CanApplyTo(skillType))
            {
                reduction += effect.cooldownReduction;
            }
        }
        return baseCooldown * (1f - Mathf.Clamp01(reduction));
    }

    public virtual float ModifyProjectileSpeed(float baseSpeed)
    {
        float multiplier = 1f;
        foreach (var effect in skillEffects.OfType<SkillStatAmplifierEffect>())
        {
            multiplier *= effect.projectileSpeedMultiplier;
        }
        return baseSpeed * multiplier;
    }

    public virtual float ModifyProjectileRange(float baseRange)
    {
        float multiplier = 1f;
        foreach (var effect in skillEffects.OfType<SkillStatAmplifierEffect>())
        {
            multiplier *= effect.rangeMultiplier;
        }
        return baseRange * multiplier;
    }

    public virtual bool IsHomingEnabled(bool baseHoming)
    {
        return baseHoming || skillEffects.OfType<HomingActivatorEffect>().Any();
    }

    public virtual float ModifyAreaRadius(float baseRadius)
    {
        float multiplier = 1f;
        foreach (var effect in skillEffects.OfType<SkillStatAmplifierEffect>())
        {
            multiplier *= effect.rangeMultiplier;
        }
        return baseRadius * multiplier;
    }

    public virtual float ModifyAreaDuration(float baseDuration)
    {
        float multiplier = 1f;
        foreach (var effect in skillEffects.OfType<SkillStatAmplifierEffect>())
        {
            multiplier *= effect.durationMultiplier;
        }
        return baseDuration * multiplier;
    }

    public virtual void OnSkillCast(Skill skill)
    {
        foreach (var effect in skillEffects)
        {
            effect.OnSkillCast(skill, skill.Owner as Player);
        }
    }

    public virtual void OnSkillHit(Skill skill, Enemy target)
    {
        foreach (var effect in skillEffects)
        {
            effect.OnSkillHit(skill, skill.Owner as Player, target);
        }
    }

    public void AddEffect(ISkillInteractionEffect effect)
    {
        if (effect != null)
        {
            skillEffects.Add(effect);
        }
    }
}
