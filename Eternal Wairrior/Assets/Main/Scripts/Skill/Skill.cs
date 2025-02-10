using UnityEngine;
using System;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] protected SkillData skillData;
    protected Vector2 fireDir;
    protected MonoBehaviour owner;

    protected virtual void Awake()
    {
    }

    public virtual void Initialize()
    {
        InitializeSkillData();
    }

    protected virtual void InitializeSkillData()
    {
        if (skillData == null || !IsValidSkillData(skillData))
        {
            skillData = new SkillData();
            Debug.Log($"Created default skill data for {gameObject.name}");
        }

    }

    protected abstract string GetDefaultSkillName();
    protected abstract string GetDefaultDescription();
    protected virtual ElementType GetDefaultElement() => ElementType.None;

    protected virtual void OnDisable()
    {
        CleanupSkill();
    }

    protected virtual void CleanupSkill()
    {
    }

    protected bool IsValidSkillData(SkillData data)

    {
        if (SkillDataManager.Instance == null || !SkillDataManager.Instance.IsInitialized ||
            SkillManager.Instance == null || !SkillManager.Instance.IsInitialized)
        {

            return true;
        }

        if (data.skillName == null) return false;
        if (data.type == SkillType.None) return false;
        if (string.IsNullOrEmpty(data.skillName)) return false;
        if (data.ID == SkillID.None) return false;

        var currentStats = data.GetCurrentTypeStat();
        if (currentStats == null) return false;
        if (currentStats.baseStat == null) return false;

        return true;
    }

    public virtual float Damage => skillData?.GetCurrentTypeStat()?.baseStat?.damage ?? 0f;
    public string SkillName => skillData?.skillName ?? "Unknown";
    protected int _skillLevel = 1;

    public int SkillLevel
    {
        get
        {
            var currentStats = GetSkillData()?.GetCurrentTypeStat()?.baseStat;
            if (currentStats != null)
            {
                return currentStats.skillLevel;
            }
            return _skillLevel;
        }
        protected set
        {
            _skillLevel = value;
            Debug.Log($"Setting skill level to {value} for {SkillName}");
        }
    }
    public int MaxSkillLevel => skillData?.GetCurrentTypeStat()?.baseStat?.maxSkillLevel ?? 1;
    public SkillID SkillID => skillData?.ID ?? SkillID.None;

    protected T GetTypeStats<T>() where T : ISkillStat
    {
        if (skillData == null) return default(T);

        var currentStats = skillData.GetCurrentTypeStat();
        if (currentStats == null) return default(T);

        if (currentStats is T typedStats)
        {
            return typedStats;
        }
        Debug.LogWarning($"Current skill is not of type {typeof(T)}");
        return default(T);
    }

    public virtual void SetSkillData(SkillData data)
    {
        skillData = data;
    }

    public virtual SkillData GetSkillData()
    {
        return skillData;
    }

    public virtual bool SkillLevelUpdate(int newLevel)
    {
        Debug.Log($"=== Starting SkillLevelUpdate for {SkillName} ===");
        Debug.Log($"Current Level: {SkillLevel}, Attempting to upgrade to: {newLevel}");

        if (newLevel <= 0)
        {
            Debug.LogError($"Invalid level: {newLevel}");
            return false;
        }

        if (newLevel > MaxSkillLevel)
        {
            Debug.LogError($"Attempted to upgrade {SkillName} beyond max level ({MaxSkillLevel})");
            return false;
        }

        if (newLevel < SkillLevel)
        {
            Debug.LogError($"Cannot downgrade skill level. Current: {SkillLevel}, Attempted: {newLevel}");
            return false;
        }

        try
        {
            var currentStats = GetSkillData()?.GetCurrentTypeStat();
            Debug.Log($"Current stats - Level: {currentStats?.baseStat?.skillLevel}, Damage: {currentStats?.baseStat?.damage}");

            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skillData.ID,
                newLevel,
                skillData.type);

            if (newStats == null)
            {
                Debug.LogError("Failed to get new stats");
                return false;
            }

            Debug.Log($"New stats received - Level: {newStats.baseStat?.skillLevel}, Damage: {newStats.baseStat?.damage}");

            newStats.baseStat.skillLevel = newLevel;
            SkillLevel = newLevel;

            Debug.Log("Setting new stats...");
            skillData.SetStatsForLevel(newLevel, newStats);

            Debug.Log("Updating skill type stats...");
            UpdateSkillTypeStats(newStats);

            Debug.Log($"=== Successfully completed SkillLevelUpdate for {SkillName} ===");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SkillLevelUpdate: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    protected virtual void UpdateSkillTypeStats(ISkillStat newStats)
    {
    }

    public virtual string GetDetailedDescription()
    {
        return skillData?.description ?? "No description available";
    }

    protected virtual void OnValidate()
    {
        if (SkillDataManager.Instance != null && SkillDataManager.Instance.IsInitialized)
        {
            if (skillData == null)
            {
                Debug.LogWarning($"SkillData is null for {GetType().Name}");
                return;
            }

            if (!IsValidSkillData(skillData))
            {
                Debug.LogError($"Invalid skill data for {GetType().Name}");
                return;
            }

            Debug.Log($"Validated skill data for {skillData.skillName}");
        }
    }

    public virtual MonoBehaviour GetOwner() => owner;
    public virtual SkillType GetSkillType() => skillData?.type ?? SkillType.None;
    public virtual ElementType GetElementType() => skillData?.element ?? ElementType.None;

    public virtual void SetOwner(MonoBehaviour newOwner)
    {
        owner = newOwner;
    }

    public virtual void ApplyItemEffect(ISkillInteractionEffect effect)
    {
        effect.ModifySkillStats(this);
    }

    public virtual void RemoveItemEffect(ISkillInteractionEffect effect)
    {
    }

    public virtual void ModifyDamage(float multiplier)
    {
        if (skillData?.GetCurrentTypeStat()?.baseStat != null)
        {
            skillData.GetCurrentTypeStat().baseStat.damage *= multiplier;
        }
    }

    public virtual void ModifyCooldown(float multiplier)
    {
    }
}
