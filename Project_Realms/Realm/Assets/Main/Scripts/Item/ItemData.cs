using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Serializable]
    public class ItemStat
    {
        public StatType statType;
        public float flatValue;
        public float percentValue;

        public string GetTooltipText()
        {
            string text = "";
            if (flatValue != 0)
                text += $"{statType} +{flatValue}\n";
            if (percentValue != 0)
                text += $"{statType} +{percentValue}%\n";
            return text;
        }
    }

    [SerializeField] private ItemID itemID;
    [SerializeField] private ItemType itemType;
    [SerializeField] private ItemRarity rarity;
    [SerializeField] private string itemName;
    [SerializeField] private string description = "";
    [SerializeField] private Sprite icon;
    [SerializeField] private List<ItemStat> stats = new List<ItemStat>();
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject worldDropPrefab;
    [SerializeField] private Color nameColor = Color.white;

    [Header("Weapon Skill")]
    [SerializeField] private Skill defaultWeaponSkill; // ⺻ ų
    public Skill DefaultWeaponSkill => defaultWeaponSkill;

    public ItemID ItemID => itemID;
    public ItemType ItemType => itemType;
    public string Description => description;
    public Sprite Icon => icon;
    public IReadOnlyList<ItemStat> Stats => stats;
    public GameObject ItemPrefab => itemPrefab;
    public GameObject WorldDropPrefab => worldDropPrefab;
    public ItemRarity Rarity => rarity;
    public string ItemName => itemName;
    public Color NameColor => nameColor;

    public string GetItemName()
    {
        return $"[{rarity}] {itemName}";
    }

    public string GetItemType()
    {
        return itemType.ToString();
    }

    public string GetDescription() 
    {
        return description;
    }

    public string GetStatsTooltip()
    {
        string tooltip = "";
        string statColor = rarity switch
        {
            ItemRarity.Common => "#FFFFFF",
            ItemRarity.Uncommon => "#1EFF00",
            ItemRarity.Rare => "#0070DD",
            ItemRarity.Epic => "#A335EE",
            ItemRarity.Legendary => "#FF8000",
            _ => "#FFFFFF"
        };

        foreach (var stat in stats)
        {
            tooltip += $"<color={statColor}>{stat.GetTooltipText()}</color>";
        }

        return tooltip.TrimEnd('\n');
    }

    public Skill GetDefaultSkillForWeapon()
    {
        if (itemType != ItemType.Sword && itemType != ItemType.Bow)
            return null;

        return defaultWeaponSkill;
    }

    public void SetRarity(ItemRarity newRarity, Color color)
    {
        rarity = newRarity;
        nameColor = color;
    }

    public void AddStat(StatType type, float flat, float percent)
    {
        stats.Add(new ItemStat
        {
            statType = type,
            flatValue = flat,
            percentValue = percent
        });
    }
}