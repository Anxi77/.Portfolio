using System.Collections.Generic;
using UnityEngine;

public class ItemInstanceData
{
    private ItemRarity rarity;
    private Color nameColor;
    private List<ItemData.ItemStat> stats = new List<ItemData.ItemStat>();
    private ItemData template;
    public string itemDesc;

    public ItemRarity Rarity => rarity;
    public Color NameColor => nameColor;
    public IReadOnlyList<ItemData.ItemStat> Stats => stats;

    public ItemInstanceData(ItemData template)
    {
        this.template = template;
        this.rarity = template.Rarity;
        this.nameColor = template.NameColor;
        this.itemDesc = template.Description;
    }

    public void SetRarity(ItemRarity newRarity, Color color)
    {
        rarity = newRarity;
        nameColor = color;
    }

    public void AddStat(StatType type, float flat, float percent)
    {
        stats.Add(new ItemData.ItemStat
        {
            statType = type,
            flatValue = flat,
            percentValue = percent
        });
    }

    public string GetItemName()
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(nameColor)}>{template.ItemName}</color>";
    }

    public string GetRarity()
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(nameColor)}>{rarity}</color>";
    }

    public string GetItemType()
    {
        return template.GetItemType();
    }

    public string GetDescription()
    {
        return template.GetDescription();
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
}