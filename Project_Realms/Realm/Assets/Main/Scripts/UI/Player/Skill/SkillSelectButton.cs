using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public class SkillSelectButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private Button button;

    private Skill skill;
    private Action<Skill> onSkillSelected;

    public void Initialize(Skill skill, Action<Skill> callback)
    {
        this.skill = skill;
        this.onSkillSelected = callback;

        skillIcon.sprite = skill.data.skillIcon;
        skillLevelText.text = $"{skill.skillStat?.GetStatValue<int>(SkillStatType.SkillLevel)}";

        button.onClick.AddListener(() => onSkillSelected?.Invoke(skill));
    }

    public void UpdateLevel(int level)
    {
        skillLevelText.text = level.ToString();
    }
}