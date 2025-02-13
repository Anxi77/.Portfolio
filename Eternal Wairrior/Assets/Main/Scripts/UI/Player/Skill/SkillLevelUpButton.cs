using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

public class SkillLevelUpButton : MonoBehaviour
{
    [SerializeField] private Image skillIconImage;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statComparisonText;
    [SerializeField] private Image elementIcon;
    [SerializeField] private Button button;

    private Action buttonClickCallback;

    public void SetSkillSelectButton(SkillData skillData, Action onClickCallback, string levelInfo = null, ISkillStat currentStats = null)
    {
        try
        {
            if (!ValidateSkillData(skillData))
            {
                SetDisabledButton("Invalid Skill Data");
                return;
            }

            buttonClickCallback = onClickCallback;
            SetupVisuals(skillData, levelInfo);
            SetupButtonListener();
            SetupStats(skillData, currentStats, levelInfo);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetSkillSelectButton: {e.Message}");
            SetDisabledButton("Error setting up button");
        }
    }

    private bool ValidateSkillData(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogError("Skill data is null");
            return false;
        }
        return true;
    }

    private void SetupSkillIcon(SkillData skillData)
    {
        if (skillIconImage == null) return;

        skillIconImage.sprite = skillData.Icon;
        skillIconImage.gameObject.SetActive(skillData.Icon != null);
    }

    private void SetupSkillText(SkillData skillData, string levelInfo)
    {
        if (skillNameText != null)
        {
            string nameText = skillData.Name;
            if (!string.IsNullOrEmpty(levelInfo))
            {
                nameText += $" ({levelInfo})";
            }
            skillNameText.text = nameText;
        }

        if (descriptionText != null)
        {
            string description = skillData.Description;
            string elementDesc = GetElementalDescription(skillData.Element);
            descriptionText.text = $"{description}\n{elementDesc}";
        }
    }

    private void SetupElementIcon(SkillData skillData)
    {
        if (elementIcon == null) return;

        var sprite = GetElementSprite(skillData.Element);
        if (sprite != null)
        {
            elementIcon.sprite = sprite;
            elementIcon.gameObject.SetActive(true);
            elementIcon.color = GetElementColor(skillData.Element);
        }
        else
        {
            elementIcon.gameObject.SetActive(false);
        }
    }

    private void SetupButtonListener()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        try
        {
            buttonClickCallback?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in button click: {e.Message}");
        }
    }

    private void SetupStats(SkillData skillData, ISkillStat currentStats, string levelInfo)
    {
        if (currentStats != null)
        {
            SetupStatComparison(skillData, currentStats);
        }
        else if (levelInfo == "New!")
        {
            SetupNewSkillStats(skillData);
        }
    }

    private void SetupNewSkillStats(SkillData skillData)
    {
        if (statComparisonText == null) return;

        var baseStats = skillData.GetCurrentTypeStat();
        if (baseStats == null) return;

        StringBuilder statText = new StringBuilder();
        statText.AppendLine("Base Stats:");
        AppendBaseStats(statText, baseStats.baseStat);
        AppendTypeSpecificStats(statText, baseStats, skillData.Type);

        statComparisonText.text = statText.ToString();
    }

    private void AppendBaseStats(StringBuilder sb, BaseSkillStat stats)
    {
        sb.AppendLine($"Base Damage: {stats.damage:F1}");
        sb.AppendLine($"Element: {stats.element}");
        sb.AppendLine($"Elemental Power: {stats.elementalPower:F1}x");
        sb.AppendLine($"Max Level: {stats.maxSkillLevel}");
    }

    private void AppendTypeSpecificStats(StringBuilder sb, ISkillStat stats, SkillType type)
    {
        sb.AppendLine("\nSkill Specific Stats:");

        switch (type)
        {
            case SkillType.Projectile:
                AppendProjectileStats(sb, stats as ProjectileSkillStat);
                break;
            case SkillType.Area:
                AppendAreaStats(sb, stats as AreaSkillStat);
                break;
            case SkillType.Passive:
                AppendPassiveStats(sb, stats as PassiveSkillStat);
                break;
        }
    }

    public void SetDisabledButton(string message)
    {
        if (skillIconImage != null) skillIconImage.gameObject.SetActive(false);
        if (elementIcon != null) elementIcon.gameObject.SetActive(false);
        if (skillNameText != null) skillNameText.text = message;
        if (descriptionText != null) descriptionText.text = "";
        if (statComparisonText != null) statComparisonText.text = "";
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = false;
        }
    }

    private string GetElementalDescription(ElementType element)
    {
        return element switch
        {
            ElementType.Dark => "Dark: Reduces defense",
            ElementType.Water => "Water: Slows movement",
            ElementType.Fire => "Fire: Deals damage over time",
            ElementType.Earth => "Earth: Stuns targets",
            _ => ""
        };
    }

    private Sprite GetElementSprite(ElementType element)
    {
        string iconPath = element switch
        {
            ElementType.Dark => "Icons/Elements/DarkElement",
            ElementType.Water => "Icons/Elements/WaterElement",
            ElementType.Fire => "Icons/Elements/FireElement",
            ElementType.Earth => "Icons/Elements/EarthElement",
            _ => ""
        };

        if (!string.IsNullOrEmpty(iconPath))
        {
            var sprite = Resources.Load<Sprite>(iconPath);
            if (sprite == null)
            {
                Debug.LogWarning($"Element sprite not found at path: {iconPath}");
            }
            return sprite;
        }
        return null;
    }

    private Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => new Color(1f, 0.3f, 0.3f),
            ElementType.Water => new Color(0.3f, 0.3f, 1f),
            ElementType.Earth => new Color(0.3f, 0.8f, 0.3f),
            ElementType.Dark => new Color(0.5f, 0.2f, 0.7f),
            _ => Color.white
        };
    }

    private void SetupStatComparison(SkillData skillData, ISkillStat currentStats)
    {
        if (statComparisonText == null) return;

        StringBuilder comparison = new StringBuilder();
        comparison.AppendLine("Stat Difference:");

        var currentLevel = currentStats.baseStat.skillLevel;
        var nextLevelStats = SkillDataManager.Instance.GetSkillStatsForLevel(
            skillData.ID,
            currentLevel + 1,
            skillData.Type);

        if (nextLevelStats == null) return;

        CompareBasicStats(comparison, currentStats.baseStat, nextLevelStats.baseStat);

        switch (skillData.Type)
        {
            case SkillType.Projectile:
                CompareProjectileStats(comparison, currentStats as ProjectileSkillStat, nextLevelStats as ProjectileSkillStat);
                break;
            case SkillType.Area:
                CompareAreaStats(comparison, currentStats as AreaSkillStat, nextLevelStats as AreaSkillStat);
                break;
            case SkillType.Passive:
                ComparePassiveStats(comparison, currentStats as PassiveSkillStat, nextLevelStats as PassiveSkillStat);
                break;
        }

        statComparisonText.text = comparison.ToString();
    }

    private void CompareBasicStats(StringBuilder sb, BaseSkillStat current, BaseSkillStat next)
    {
        float damageDiff = next.damage - current.damage;
        if (damageDiff != 0)
        {
            sb.AppendLine($"Damage : {current.damage:F1}  {next.damage:F1} (<color={GetColorForDiff(damageDiff)}>{"+" + damageDiff:F1}</color>)");
        }

        float elementalPowerDiff = next.elementalPower - current.elementalPower;
        if (elementalPowerDiff != 0)
        {
            sb.AppendLine($"Elemental Power: {current.elementalPower:F1}  {next.elementalPower:F1} (<color={GetColorForDiff(elementalPowerDiff)}>{"+" + elementalPowerDiff:F1}</color>)");
        }
    }

    private void CompareProjectileStats(StringBuilder sb, ProjectileSkillStat current, ProjectileSkillStat next)
    {
        if (current == null || next == null) return;

        CompareFloatValue(sb, "Projectile Speed", current.projectileSpeed, next.projectileSpeed);
        CompareFloatValue(sb, "Scale", current.projectileScale, next.projectileScale, false, "x");
        CompareFloatValue(sb, "Shot Interval", current.shotInterval, next.shotInterval, true, "s");
        CompareIntValue(sb, "Pierce Count", current.pierceCount, next.pierceCount);
        CompareFloatValue(sb, "Attack Range", current.attackRange, next.attackRange);
        CompareFloatValue(sb, "Homing Range", current.homingRange, next.homingRange);
        CompareIntValue(sb, "Projectile Count", current.projectileCount, next.projectileCount);
        CompareFloatValue(sb, "Inner Interval", current.innerInterval, next.innerInterval, true, "s");
        CompareFloatValue(sb, "Explosion Radius", current.explosionRad, next.explosionRad);
    }

    private void CompareAreaStats(StringBuilder sb, AreaSkillStat current, AreaSkillStat next)
    {
        if (current == null || next == null) return;

        CompareFloatValue(sb, "Radius", current.radius, next.radius);
        CompareFloatValue(sb, "Duration", current.duration, next.duration, false, "s");
        CompareFloatValue(sb, "Tick Rate", current.tickRate, next.tickRate, true, "s");
        CompareFloatValue(sb, "Move Speed", current.moveSpeed, next.moveSpeed);
        CompareBoolValue(sb, "Persistence", current.isPersistent, next.isPersistent);
    }

    private void ComparePassiveStats(StringBuilder sb, PassiveSkillStat current, PassiveSkillStat next)
    {
        if (current == null || next == null)
        {
            Debug.LogWarning("Cannot compare passive stats: current or next is null");
            return;
        }

        try
        {
            // 기본 지속시간과 쿨다운
            if (next.effectDuration != current.effectDuration)
            {
                CompareFloatValue(sb, "Effect Duration", current.effectDuration, next.effectDuration, false, "s");
            }
            if (next.cooldown != current.cooldown)
            {
                CompareFloatValue(sb, "Cooldown", current.cooldown, next.cooldown, true, "s");
            }

            // 증가량 비교 (0이 아닌 경우에만 표시)
            ComparePassiveIncrease(sb, "Damage", current.damageIncrease, next.damageIncrease);
            ComparePassiveIncrease(sb, "Defense", current.defenseIncrease, next.defenseIncrease);
            ComparePassiveIncrease(sb, "Move Speed", current.moveSpeedIncrease, next.moveSpeedIncrease);
            ComparePassiveIncrease(sb, "Attack Speed", current.attackSpeedIncrease, next.attackSpeedIncrease);
            ComparePassiveIncrease(sb, "Attack Range", current.attackRangeIncrease, next.attackRangeIncrease);
            ComparePassiveIncrease(sb, "HP", current.hpIncrease, next.hpIncrease);
            ComparePassiveIncrease(sb, "HP Regen", current.hpRegenIncrease, next.hpRegenIncrease);
            ComparePassiveIncrease(sb, "EXP Collection Range", current.expAreaIncrease, next.expAreaIncrease);

            // 트리거 확률이 100% 미만일 때만 표시
            if (next.triggerChance < 100 || current.triggerChance < 100)
            {
                CompareFloatValue(sb, "Trigger Chance", current.triggerChance, next.triggerChance, false, "%");
            }

            // 유도 효과 변경 표시
            if (current.homingActivate != next.homingActivate)
            {
                string status = next.homingActivate ? "Activated" : "Deactivated";
                string color = next.homingActivate ? "#00FF00" : "#FF0000";
                sb.AppendLine($"Homing Effect: <color={color}>{status}</color>");
            }

            // 디버그 로그 추가
            Debug.Log($"Comparing passive stats:\n" +
                     $"Current - Damage: {current.damageIncrease}, Speed: {current.moveSpeedIncrease}\n" +
                     $"Next - Damage: {next.damageIncrease}, Speed: {next.moveSpeedIncrease}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error comparing passive stats: {e.Message}");
        }
    }

    private void ComparePassiveIncrease(StringBuilder sb, string statName, float current, float next)
    {
        if (current > 0 || next > 0)
        {
            float diff = next - current;
            string color = GetColorForDiff(diff);
            string diffText = diff > 0 ? $"+{diff:F1}" : $"{diff:F1}";
            sb.AppendLine($"{statName} Increase: {current:F1}% → {next:F1}% " +
                         $"(<color={color}>{diffText}%</color>)");
        }
    }

    private void CompareFloatValue(StringBuilder sb, string statName, float current, float next, bool isNegativeBetter = false, string unit = "")
    {
        float diff = next - current;
        if (diff != 0)
        {
            string color = isNegativeBetter ? GetColorForDiff(-diff) : GetColorForDiff(diff);
            string diffText = diff > 0 ? $"+{diff:F1}" : $"{diff:F1}";
            sb.AppendLine($"{statName}: {current:F1}{unit}  {next:F1}{unit} (<color={color}>{diffText}{unit}</color>)");
        }
    }

    private void CompareIntValue(StringBuilder sb, string statName, int current, int next, bool isNegativeBetter = false)
    {
        int diff = next - current;
        if (diff != 0)
        {
            string color = isNegativeBetter ? GetColorForDiff(-diff) : GetColorForDiff(diff);
            sb.AppendLine($"{statName}: {current}  {next} (<color={color}>{(diff > 0 ? "+" : "")}{diff}</color>)");
        }
    }

    private string GetColorForDiff(float diff)
    {
        return diff > 0 ? "#00FF00" : "#FF0000";
    }

    private void CompareBoolValue(StringBuilder sb, string statName, bool current, bool next)
    {
        if (current != next)
        {
            string color = next ? "#00FF00" : "#FF0000";
            sb.AppendLine($"{statName}: {current}  {next} (<color={color}>{(next ? "Activated" : "Deactivated")}</color>)");
        }
    }

    private void AppendProjectileStats(StringBuilder sb, ProjectileSkillStat stats)
    {
        if (stats == null) return;

        if (stats.projectileSpeed > 0)
            sb.AppendLine($"Attack Range: {stats.attackRange:F1}");
        if (stats.shotInterval > 0)
            sb.AppendLine($"Shot Interval: {stats.shotInterval:F1}s");
        if (stats.projectileSpeed > 0)
            sb.AppendLine($"Projectile Speed: {stats.projectileSpeed:F1}");
        if (stats.pierceCount > 0)
            sb.AppendLine($"Pierce Count: {stats.pierceCount}");
        if (stats.isHoming)
            sb.AppendLine($"Homing Range: {stats.homingRange:F1}");
        if (stats.explosionRad > 0)
            sb.AppendLine($"Explosion Radius: {stats.explosionRad:F1}");
        if (stats.projectileCount > 1)
            sb.AppendLine($"Projectile Count: {stats.projectileCount}");
    }

    private void AppendAreaStats(StringBuilder sb, AreaSkillStat stats)
    {
        if (stats == null) return;

        if (stats.radius > 0)
            sb.AppendLine($"Area Radius: {stats.radius:F1}");
        if (stats.duration > 0)
            sb.AppendLine($"Duration: {stats.duration:F1}s");
        if (stats.tickRate > 0)
            sb.AppendLine($"Tick Rate: {stats.tickRate:F1}s");
        if (stats.moveSpeed > 0)
            sb.AppendLine($"Move Speed: {stats.moveSpeed:F1}");
        if (stats.isPersistent)
            sb.AppendLine("Persistent Effect");
    }

    private void AppendPassiveStats(StringBuilder sb, PassiveSkillStat stats)
    {
        if (stats == null) return;

        if (stats.damageIncrease > 0)
            sb.AppendLine($"Damage Increase: {stats.damageIncrease:F1}%");
        if (stats.defenseIncrease > 0)
            sb.AppendLine($"Defense Increase: {stats.defenseIncrease:F1}%");
        if (stats.moveSpeedIncrease > 0)
            sb.AppendLine($"Move Speed Increase: {stats.moveSpeedIncrease:F1}%");
        if (stats.attackSpeedIncrease > 0)
            sb.AppendLine($"Attack Speed Increase: {stats.attackSpeedIncrease:F1}%");
        if (stats.attackRangeIncrease > 0)
            sb.AppendLine($"Attack Range Increase: {stats.attackRangeIncrease:F1}%");
        if (stats.hpIncrease > 0)
            sb.AppendLine($"HP Increase: {stats.hpIncrease:F1}%");
        if (stats.hpRegenIncrease > 0)
            sb.AppendLine($"HP Regen Increase: {stats.hpRegenIncrease:F1}%");
        if (stats.expAreaIncrease > 0)
            sb.AppendLine($"EXP Collection Range: {stats.expAreaIncrease:F1}%");
        if (stats.effectDuration > 0)
            sb.AppendLine($"Effect Duration: {stats.effectDuration:F1}s");
        if (stats.cooldown > 0)
            sb.AppendLine($"Cooldown: {stats.cooldown:F1}s");
        if (stats.triggerChance < 100)
            sb.AppendLine($"Trigger Chance: {stats.triggerChance:F1}%");
        if (stats.homingActivate)
            sb.AppendLine("Activates Homing Effect");
    }

    private void SetupVisuals(SkillData skillData, string levelInfo)
    {
        SetupSkillIcon(skillData);
        SetupSkillText(skillData, levelInfo);
        SetupElementIcon(skillData);
    }
}
