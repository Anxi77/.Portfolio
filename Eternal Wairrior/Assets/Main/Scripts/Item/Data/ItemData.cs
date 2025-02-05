using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;
using System;
using Newtonsoft.Json;

[Serializable]
public class ItemData
{
    // 기본 아이템 데이터
    [JsonProperty] public string ID { get; set; }
    [JsonProperty] public string Name { get; set; }
    [JsonProperty] public string Description { get; set; }
    [JsonProperty] public ItemType Type { get; set; }
    [JsonProperty] public ItemRarity Rarity { get; set; }
    [JsonProperty] public ElementType Element { get; set; }
    [JsonProperty] public int MaxStack { get; set; } = 1;
    [JsonProperty] public float DropRate { get; set; }
    [JsonProperty] public int MinAmount { get; set; } = 1;
    [JsonProperty] public int MaxAmount { get; set; } = 1;
    [JsonProperty] public List<StatType> BaseStatTypes { get; set; } = new();
    [JsonProperty] public string IconPath { get; set; }
    [JsonProperty] public ItemStatRangeData StatRanges { get; set; } = new();
    [JsonProperty] public List<StatContainer> Stats { get; set; } = new();
    [JsonProperty] public ItemEffectRangeData EffectRanges { get; set; } = new();
    [JsonProperty] public List<ItemEffectData> Effects { get; set; } = new();
    [JsonProperty] public Dictionary<string, float> EffectValues { get; set; } = new();

    // 런타임 전용 데이터
    [NonSerialized] private Sprite _icon;
    [JsonIgnore]
    public Sprite Icon => _icon ??= !string.IsNullOrEmpty(IconPath) ? Resources.Load<Sprite>(IconPath) : null;

    // 런타임 데이터
    [JsonIgnore] public int amount = 1;

    #region Stats Management
    public void AddStat(StatContainer stat)
    {
        if (stat == null) return;
        Stats.RemoveAll(s => s.statType == stat.statType);
        Stats.Add(stat);
    }

    public StatContainer GetStat(StatType statType) =>
        Stats.FirstOrDefault(s => s.statType == statType);

    public float GetStatValue(StatType statType) =>
        GetStat(statType)?.amount ?? 0f;

    public void ClearStats() => Stats.Clear();
    #endregion

    #region Effects Management
    public void AddEffect(ItemEffectData effect)
    {
        if (effect == null) return;
        Effects.Add(effect);
    }

    public void RemoveEffect(string effectId) =>
        Effects.RemoveAll(e => e.effectId == effectId);

    public ItemEffectData GetEffect(string effectId) =>
        Effects.FirstOrDefault(e => e.effectId == effectId);

    public List<ItemEffectData> GetEffectsByType(EffectType type) =>
        Effects.Where(e => e.effectType == type).ToList();

    public List<ItemEffectData> GetEffectsForSkill(SkillType skillType) =>
        Effects.Where(e => e.applicableSkills?.Contains(skillType) ?? false).ToList();

    public List<ItemEffectData> GetEffectsForElement(ElementType elementType) =>
        Effects.Where(e => e.applicableElements?.Contains(elementType) ?? false).ToList();
    #endregion

    #region Cloning
    /// <summary>
    /// 아이템 데이터의 깊은 복사본을 생성.
    /// JSON 직렬화/역직렬화를 사용하여 모든 중첩된 객체들의 새로운 인스턴스를 생성.
    /// </summary>
    public ItemData Clone()
    {
        return JsonConvert.DeserializeObject<ItemData>(
            JsonConvert.SerializeObject(this)
        );
    }
    #endregion
}
