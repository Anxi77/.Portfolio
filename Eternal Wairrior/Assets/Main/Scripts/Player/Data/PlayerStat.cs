using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StatType
{
    // ⺻
    None,
    MaxHp,              // ִ ü
    CurrentHp,          // ü
    Damage,             // ݷ
    Defense,            // 
    MoveSpeed,          // ̵ ӵ
    AttackSpeed,        // ӵ
    AttackRange,        // 
    AttackAngle,        // 

    // Ư
    ExpCollectionRadius,// ġ ȹ
    HpRegenRate,       // HP 
    ExpGainRate,       // ġ ȹ淮
    GoldGainRate,      // ȹ淮
    CriticalChance,    // ġŸ Ȯ
    CriticalDamage,    // ġŸ 

    // 
    FireResistance,    // ȭ
    IceResistance,     // 
    LightningResistance,// 
    PoisonResistance,  // 

    // ̻
    StunResistance,    // 
    SlowResistance,    // ο

    // Ÿ
    Luck,              //  (  )
    DodgeChance,       // ȸ
    ReflectDamage,     // 
    LifeSteal,         // 
}

public enum SourceType
{
    None,       // 없음
    Base,       // 기본 
    Level,      // 레벨 효과 
    Passive,    // 패시브 
    Active,     // 액티브 
    Weapon,    // 무기
    Armor,     // 방어구
    Accessory, // 장신구
    Special,   // 특수
    Consumable, // 소비
    Buff,       // 버프
    Debuff      // 디버프
}

public enum IncreaseType
{
    Add,    // ϱ
    Mul     // ϱ
}

[System.Serializable]
public class StatContainer
{
    public StatType statType;
    public SourceType sourceType;
    public IncreaseType increaseType;
    public float amount;

    public StatContainer()
    {
        statType = StatType.None;
        sourceType = SourceType.None;
        increaseType = IncreaseType.Add;
        amount = 0f;
    }

    public StatContainer(StatType statType, SourceType sourceType, IncreaseType increaseType, float amount)
    {
        this.statType = statType;
        this.sourceType = sourceType;
        this.increaseType = increaseType;
        this.amount = amount;
    }

    public override bool Equals(object obj)
    {
        if (obj is StatContainer other)
        {
            return statType == other.statType &&
                   sourceType == other.sourceType &&
                   increaseType == other.increaseType &&
                   amount == other.amount;
        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + statType.GetHashCode();
            hash = hash * 23 + sourceType.GetHashCode();
            hash = hash * 23 + increaseType.GetHashCode();
            hash = hash * 23 + amount.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(StatContainer left, StatContainer right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    public static bool operator !=(StatContainer left, StatContainer right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"[{sourceType}] {statType} {(increaseType == IncreaseType.Add ? "+" : "x")} {amount}";
    }
}

public class PlayerStat : MonoBehaviour
{
    [SerializeField] private PlayerStatData baseData;
    private Dictionary<EquipmentSlot, List<StatContainer>> equippedItems = new();
    private Player player;

    private Dictionary<StatType, float> currentStats = new();
    private Dictionary<SourceType, List<StatContainer>> activeEffects = new();

    private void Awake()
    {
        player = GetComponent<Player>();

        // baseData가 null일 때 GameManager에서 가져오기
        if (baseData == null)
        {
            baseData = PlayerDataManager.Instance.CurrentPlayerStatData;
            if (baseData == null)
            {
                Debug.LogWarning("PlayerStatData is null, loading from Resources...");
                baseData = Resources.Load<PlayerStatData>("DefaultPlayerStats");

                if (baseData == null)
                {
                    Debug.LogError("Could not load DefaultPlayerStats, creating new instance with default values");
                    baseData = ScriptableObject.CreateInstance<PlayerStatData>();
                    SetDefaultValues(baseData);
                }
            }
        }

        InitializeStats();
    }

    private void SetDefaultValues(PlayerStatData data)
    {
        data.baseHp = 100f;
        data.baseDamage = 10f;
        data.baseDefense = 5f;
        data.baseSpeed = 5f;
        data.baseAttackSpeed = 1f;
        data.baseAttackRange = 2f;
        data.baseAttackAngle = 120f;
        data.baseExpCollectionRadius = 3f;
        data.baseHpRegenRate = 1f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        SaveCurrentState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestoreState();
    }

    public void SaveCurrentState()
    {
        baseData.SavePermanentStats();
    }

    public void LoadSavedState(PlayerStatData savedData)
    {
        baseData = savedData;
        InitializeStats();
    }

    public void RestoreState()
    {
        InitializeStats();
    }

    public void InitializeStats()
    {
        // Dictionary 초기화 전에 먼저 비우기
        currentStats.Clear();
        activeEffects.Clear();

        Debug.Log($"Initializing stats with baseHp: {baseData.baseHp}");

        // 기본 스탯 설정
        currentStats[StatType.MaxHp] = baseData.baseHp;
        currentStats[StatType.CurrentHp] = baseData.baseHp;  // CurrentHp도 MaxHp와 동일하게 설정
        currentStats[StatType.Damage] = baseData.baseDamage;
        currentStats[StatType.MoveSpeed] = baseData.baseSpeed;
        currentStats[StatType.Defense] = baseData.baseDefense;
        currentStats[StatType.AttackSpeed] = baseData.baseAttackSpeed;
        currentStats[StatType.AttackRange] = baseData.baseAttackRange;
        currentStats[StatType.AttackAngle] = baseData.baseAttackAngle;
        currentStats[StatType.ExpCollectionRadius] = baseData.baseExpCollectionRadius;
        currentStats[StatType.HpRegenRate] = baseData.baseHpRegenRate;

        Debug.Log($"After initialization - MaxHp: {currentStats[StatType.MaxHp]}, CurrentHp: {currentStats[StatType.CurrentHp]}");

        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            if (!currentStats.ContainsKey(statType))
            {
                currentStats[statType] = 0f;  // 기본값 0으로 설정
            }
        }

        foreach (var effect in baseData.GetAllPermanentStats())
        {
            if (!activeEffects.ContainsKey(effect.sourceType))
                activeEffects[effect.sourceType] = new List<StatContainer>();

            activeEffects[effect.sourceType].Add(effect);
        }

        RecalculateStats();

        Debug.Log($"After RecalculateStats - MaxHp: {currentStats[StatType.MaxHp]}, CurrentHp: {currentStats[StatType.CurrentHp]}");
    }

    public void AddStatModifier(StatType statType, SourceType source, IncreaseType incType, float amount)
    {
        var container = new StatContainer(statType, source, incType, amount);

        if (!activeEffects.ContainsKey(source))
            activeEffects[source] = new List<StatContainer>();

        activeEffects[source].Add(container);

        if (IsPermanentSource(source))
        {
            baseData.AddPermanentStat(container);
        }

        RecalculateStats();
    }

    private bool IsPermanentSource(SourceType source)
    {
        return source == SourceType.Weapon ||
               source == SourceType.Armor ||
               source == SourceType.Accessory ||
               source == SourceType.Special;
    }

    public float GetStat(StatType type)
    {
        if (!currentStats.ContainsKey(type))
        {
            currentStats[type] = 0f;  // 없는 스탯은 0으로 초기화
        }
        return currentStats[type];
    }

    private void RecalculateStats()
    {
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            float baseValue = GetBaseValue(statType);
            float addValue = 0;
            float mulValue = 1f;

            foreach (var effectList in activeEffects.Values)
            {
                foreach (var effect in effectList)
                {
                    if (effect.statType != statType) continue;

                    if (effect.increaseType == IncreaseType.Add)
                        addValue += effect.amount;
                    else
                        mulValue *= (1 + effect.amount);
                }
            }

            currentStats[statType] = (baseValue + addValue) * mulValue;
        }
    }

    public void RemoveStatsBySource(SourceType source)
    {
        if (activeEffects.ContainsKey(source))
        {
            activeEffects[source].Clear();

            if (IsPermanentSource(source))
            {
                baseData.RemovePermanentStatsBySource(source);
            }
        }

        RecalculateStats();
    }

    private bool isHomingActivated = false;

    public void ActivateHoming(bool activate)
    {
        isHomingActivated = activate;

        if (player.skills == null) return;

        foreach (var skill in player.skills)
        {
            if (skill is ProjectileSkills projectileSkill)
            {
                var skillData = projectileSkill.GetSkillData();
                if (skillData != null && skillData.metadata.Type == SkillType.Projectile)
                {
                    projectileSkill.UpdateHomingState(activate);
                }
            }
        }
    }

    public bool IsHomingActivated() => isHomingActivated;

    public void UpdateStatsForLevel(int level)
    {
        AddStatModifier(StatType.MaxHp, SourceType.Level, IncreaseType.Add,
            baseData.hpIncreasePerLevel * (level - 1));

        AddStatModifier(StatType.Damage, SourceType.Level, IncreaseType.Add,
            baseData.damageIncreasePerLevel * (level - 1));

        AddStatModifier(StatType.MoveSpeed, SourceType.Level, IncreaseType.Add,
            baseData.speedIncreasePerLevel * (level - 1));

        AddStatModifier(StatType.Defense, SourceType.Level, IncreaseType.Add,
            baseData.defenseIncreasePerLevel * (level - 1));
    }

    private float GetBaseValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHp:
                return baseData.baseHp;
            case StatType.Damage:
                return baseData.baseDamage;
            case StatType.MoveSpeed:
                return baseData.baseSpeed;
            case StatType.Defense:
                return baseData.baseDefense;
            case StatType.AttackSpeed:
                return baseData.baseAttackSpeed;
            case StatType.AttackRange:
                return baseData.baseAttackRange;
            case StatType.ExpCollectionRadius:
                return baseData.baseExpCollectionRadius;
            case StatType.HpRegenRate:
                return baseData.baseHpRegenRate;
            default:
                return 0f;
        }
    }

    public void ResetToBase()
    {
        // 모든 효과 제거
        activeEffects.Clear();
        equippedItems.Clear();

        // 기본 스탯으로 초기화
        InitializeStats();

        // 체력을 최대치로 설정
        float maxHp = GetStat(StatType.MaxHp);
        SetCurrentHp(maxHp);

        Debug.Log($"After ResetToBase - MaxHp: {maxHp}, CurrentHp: {currentStats[StatType.CurrentHp]}");
    }

    public void SetCurrentHp(float value)
    {
        currentStats[StatType.CurrentHp] = Mathf.Clamp(value, 0, currentStats[StatType.MaxHp]);
    }

    public void ToggleSourceEffects(SourceType source, bool enable)
    {
        if (!enable)
        {
            if (activeEffects.ContainsKey(source))
            {
                activeEffects[source].Clear();
            }

            if (IsPermanentSource(source))
            {
                foreach (var stat in baseData.GetPermanentStats(source))
                {
                    if (!activeEffects.ContainsKey(source))
                        activeEffects[source] = new List<StatContainer>();
                    activeEffects[source].Add(stat);
                }
            }
        }

        RecalculateStats();
    }

    public void EquipItem(List<StatContainer> itemStats, EquipmentSlot slot)
    {
        UnequipItem(slot);

        equippedItems[slot] = itemStats;
        foreach (var stat in itemStats)
        {
            AddStatModifier(stat.statType, stat.sourceType, stat.increaseType, stat.amount);
        }
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (equippedItems.TryGetValue(slot, out var existingStats))
        {
            foreach (var stat in existingStats)
            {
                RemoveSpecificStat(stat);
            }
            equippedItems.Remove(slot);
        }
    }

    public void ToggleEquipmentSlot(EquipmentSlot slot, bool enable)
    {
        if (equippedItems.TryGetValue(slot, out var stats))
        {
            if (!enable)
            {
                foreach (var stat in stats)
                {
                    RemoveSpecificStat(stat);
                }
            }
            else
            {
                foreach (var stat in stats)
                {
                    AddStatModifier(stat.statType, stat.sourceType, stat.increaseType, stat.amount);
                }
            }
        }
    }

    private void RemoveSpecificStat(StatContainer stat)
    {
        if (activeEffects.TryGetValue(stat.sourceType, out var effects))
        {
            effects.RemoveAll(x =>
                x.statType == stat.statType &&
                x.increaseType == stat.increaseType &&
                x.amount == stat.amount);
        }
        RecalculateStats();
    }

    public List<StatContainer> GetActiveEffects(SourceType source)
    {
        if (activeEffects.TryGetValue(source, out var effects))
        {
            return new List<StatContainer>(effects);
        }
        return new List<StatContainer>();
    }

    public void RemoveStatModifier(StatType statType, SourceType source)
    {
        if (activeEffects.TryGetValue(source, out var effects))
        {
            effects.RemoveAll(x => x.statType == statType);
            RecalculateStats();
        }
    }

    // 레벨과 경험치 관련
    public int level { get; set; } = 1;
    public float currentExp { get; set; } = 0f;
    public float currentHp
    {
        get => currentStats[StatType.CurrentHp];
        set => SetCurrentHp(value);
    }

    // 새로운 메서드들
    public void RestoreFullHealth()
    {
        float maxHp = GetStat(StatType.MaxHp);
        SetCurrentHp(maxHp);
    }

    public void LoadStats(PlayerStatData data)
    {
        if (data == null) return;
        baseData = data;
        InitializeStats();
    }

    public float GetFinalStatValue(StatType statType)
    {
        return GetStat(statType);
    }

    public void RemoveStatModifier(StatType statType, SourceType source, IncreaseType incType, float amount)
    {
        if (activeEffects.TryGetValue(source, out var effects))
        {
            effects.RemoveAll(x =>
                x.statType == statType &&
                x.increaseType == incType &&
                x.amount == amount);
            RecalculateStats();
        }
    }

    // GetStatData 메서드 추가
    public PlayerStatData GetStatData()
    {
        // 현재 스탯 데이터의 복사본을 생성
        PlayerStatData newData = ScriptableObject.CreateInstance<PlayerStatData>();

        // 기본 스탯 값들을 복사
        newData.baseHp = baseData.baseHp;
        newData.baseDamage = baseData.baseDamage;
        newData.baseDefense = baseData.baseDefense;
        newData.baseSpeed = baseData.baseSpeed;
        newData.baseAttackSpeed = baseData.baseAttackSpeed;
        newData.baseAttackRange = baseData.baseAttackRange;
        newData.baseAttackAngle = baseData.baseAttackAngle;
        newData.baseExpCollectionRadius = baseData.baseExpCollectionRadius;
        newData.baseHpRegenRate = baseData.baseHpRegenRate;

        // 레벨당 증가량 복사
        newData.hpIncreasePerLevel = baseData.hpIncreasePerLevel;
        newData.damageIncreasePerLevel = baseData.damageIncreasePerLevel;
        newData.speedIncreasePerLevel = baseData.speedIncreasePerLevel;
        newData.defenseIncreasePerLevel = baseData.defenseIncreasePerLevel;

        // 영구적인 스탯 효과들 복사
        foreach (var effect in activeEffects)
        {
            if (IsPermanentSource(effect.Key))
            {
                foreach (var stat in effect.Value)
                {
                    newData.AddPermanentStat(stat);
                }
            }
        }

        return newData;
    }
}