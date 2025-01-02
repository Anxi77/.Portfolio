using System.Collections.Generic;
using UnityEngine;

public class Debuff : MonoBehaviour
{
    private DeBuffSkillStat deBuffSkillStat;
    private List<Unit> debuffTargets = new List<Unit>();
    [SerializeField] protected StatType statType;
    [SerializeField] protected StatModifierType modifierType;
    private bool isOwnerPlayer = false;
    private Unit owner;

    public void Initialize(Unit owner,SkillStat skillStat)
    {
        this.owner = owner;
        deBuffSkillStat = (DeBuffSkillStat)skillStat;
        deBuffSkillStat.InitializeStats();
        this.transform.localScale = Vector3.one * deBuffSkillStat.GetStatValue<float>(SkillStatType.DeBuffAreaScale);
        isOwnerPlayer = owner is Player;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Unit>(out Unit targetUnit))
        {
            if ((isOwnerPlayer && targetUnit is Monster) || (!isOwnerPlayer && targetUnit is Player))
            {
                debuffTargets.Add(targetUnit);
                SetDeBuff(targetUnit, statType, deBuffSkillStat.GetStatValue<float>(SkillStatType.BuffValue), modifierType);
                print($"{targetUnit} 에게 디버프 적용");
                print($"{targetUnit.CharacterStats.GetStatValue(statType)} 디버프후 스탯");
            }
        }  
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Unit>(out Unit targetUnit))
        {
            if ((isOwnerPlayer && targetUnit is Monster) || (!isOwnerPlayer && targetUnit is Player))
            {

                RemoveDeBuff(targetUnit, statType);
                if (debuffTargets.Contains(targetUnit))
                {
                    debuffTargets.Remove(targetUnit);
                }
                print($"{targetUnit} 에게 디버프 해제");
            }
        }
    }

    protected virtual void OnDisable()
    {
        foreach (Unit debuffTarget in debuffTargets)
        {
            if (debuffTargets != null)
            {
                RemoveDeBuff(debuffTarget, statType);
            }
        }
        debuffTargets.Clear();
    }


    protected virtual void SetDeBuff(Unit target, StatType statType, float value, StatModifierType modType)
    {
        StatModifier statModifier = new StatModifier(-value, modType, this, SourceType.Debuff);
        target.CharacterStats.AddModifier(statType, statModifier);
        UIManager.Instance.PlayerUI.statUI.UpdateUI();
        target.UpdateMoveSpeed();
    }

    protected virtual void RemoveDeBuff(Unit target, StatType statType)
    {
        target.CharacterStats.GetStat(statType)?.RemoveAllModifiersFromSource(this);
        target.UpdateMoveSpeed();
        UIManager.Instance.PlayerUI.statUI.UpdateUI();
        print($"{target.CharacterStats.GetStatValue(statType)} 디버프제거후 스탯");
    }
}
