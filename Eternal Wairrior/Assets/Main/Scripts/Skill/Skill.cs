using UnityEngine;
using System;

[Serializable]
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
            skillData = new SkillData
            {
                metadata = new SkillMetadata
                {
                    Type = GetSkillType(),
                    Name = GetDefaultSkillName(),
                    Description = GetDefaultDescription(),
                    Element = GetDefaultElement(),
                    Tier = 1
                }
            };
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
        // ڽ Ŭ
    }

    protected bool IsValidSkillData(SkillData data)
    {
        // SkillDataManager SkillManager ʱȭ ʱ
        if (SkillDataManager.Instance == null || !SkillDataManager.Instance.IsInitialized ||
            SkillManager.Instance == null || !SkillManager.Instance.IsInitialized)
        {
            return true;
        }

        if (data.metadata == null) return false;
        if (data.metadata.Type == SkillType.None) return false;
        if (string.IsNullOrEmpty(data.metadata.Name)) return false;
        if (data.metadata.ID == SkillID.None) return false;

        // ų ŸԺ ʼ
        var currentStats = data.GetCurrentTypeStat();
        if (currentStats == null) return false;
        if (currentStats.baseStat == null) return false;

        return true;
    }

    // ⺻ 
    public virtual float Damage => skillData?.GetCurrentTypeStat()?.baseStat?.damage ?? 0f;
    public string SkillName => skillData?.metadata?.Name ?? "Unknown";
    protected int _skillLevel = 1;  // ⺻ ʵ
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
    public SkillID SkillID => skillData?.metadata?.ID ?? SkillID.None;

    // ŸԺ 
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

    // Unity νͿ ֵ ޼
    public virtual void SetSkillData(SkillData data)
    {
        skillData = data;
    }

    // ʹ ų 
    public virtual SkillData GetSkillData()
    {
        return skillData;
    }

    public virtual bool SkillLevelUpdate(int newLevel)
    {
        Debug.Log($"=== Starting SkillLevelUpdate for {SkillName} ===");
        Debug.Log($"Current Level: {SkillLevel}, Attempting to upgrade to: {newLevel}");

        // ȿ˻
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

        // ʹ ų ǳʶ
        if (newLevel < SkillLevel)
        {
            Debug.LogError($"Cannot downgrade skill level. Current: {SkillLevel}, Attempted: {newLevel}");
            return false;
        }

        try
        {
            // ʹ 
            var currentStats = GetSkillData()?.GetCurrentTypeStat();
            Debug.Log($"Current stats - Level: {currentStats?.baseStat?.skillLevel}, Damage: {currentStats?.baseStat?.damage}");

            // ʹ 
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skillData.metadata.ID,
                newLevel,
                skillData.metadata.Type);

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
        return skillData?.metadata?.Description ?? "No description available";
    }

    protected virtual void OnValidate()
    {
        // Application.isPlaying üũ, ʱȭ Ϸ 쿡 ϵ
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

            Debug.Log($"Validated skill data for {skillData.metadata.Name}");
        }
    }

    public virtual MonoBehaviour GetOwner() => owner;
    public virtual SkillType GetSkillType() => skillData?.metadata?.Type ?? SkillType.None;
    public virtual ElementType GetElementType() => skillData?.metadata?.Element ?? ElementType.None;

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
        // 효과 제거 로직
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
        // 쿨다운 수정 로직 구현
    }
}
