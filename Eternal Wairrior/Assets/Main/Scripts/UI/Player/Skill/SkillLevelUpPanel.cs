using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class SkillLevelUpPanel : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform list;
    public SkillLevelUpButton buttonPrefab;
    public SkillMaxLevelButton maxLevelButtonPrefab;

    [Header("Settings")]
    private const int SKILL_CHOICES = 3;

    [Header("Error Handling")]
    [SerializeField] private GameObject errorPopup;
    [SerializeField] private TextMeshProUGUI errorText;

    private Action<Skill> skillSelectedCallback;

    private void OnEnable() => Time.timeScale = 0f;
    private void OnDisable() => Time.timeScale = 1f;

    public void LevelUpPanelOpen(List<Skill> playerSkills, Action<Skill> onSkillSelected)
    {
        try
        {
            skillSelectedCallback = onSkillSelected;
            ClearExistingButtons();
            gameObject.SetActive(true);

            var availableSkills = GetAvailableSkills(playerSkills);
            if (!availableSkills.Any())
            {
                ShowNoSkillsAvailable();
                return;
            }

            foreach (var skillData in availableSkills)
            {
                CreateSkillUpgradeButton(skillData, playerSkills);
            }

            ShowElementalHeader(availableSkills[0].Element);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in LevelUpPanelOpen: {e.Message}");
            ShowError("Failed to open level up panel");
        }
    }

    private List<SkillData> GetAvailableSkills(List<Skill> playerSkills)
    {
        return SkillManager.Instance.GetRandomSkills(SKILL_CHOICES)
            .Where(skillData => IsSkillAvailable(skillData, playerSkills))
            .ToList();
    }

    private bool IsSkillAvailable(SkillData skillData, List<Skill> playerSkills)
    {
        if (!ValidateSkillData(skillData)) return false;

        var existingSkill = playerSkills.Find(s => s.skillData.ID == skillData.ID);
        return existingSkill == null || existingSkill.skillData.GetCurrentTypeStat().baseStat.skillLevel < existingSkill.skillData.GetCurrentTypeStat().baseStat.maxSkillLevel;
    }

    private void CreateSkillUpgradeButton(SkillData skillData, List<Skill> playerSkills)
    {
        var existingSkill = playerSkills.Find(s => s.skillData.ID == skillData.ID);
        var button = Instantiate(buttonPrefab, list);
        var upgradeInfo = GetUpgradeInfo(existingSkill, skillData);

        button.SetSkillSelectButton(
            skillData,
            CreateButtonCallback(skillData, existingSkill),
            upgradeInfo.levelText,
            upgradeInfo.currentStats
        );
    }

    private Action CreateButtonCallback(SkillData skillData, Skill existingSkill)
    {
        return new Action(() => OnSkillButtonClicked(skillData, existingSkill));
    }

    private (string levelText, ISkillStat currentStats) GetUpgradeInfo(Skill existingSkill, SkillData skillData)
    {
        if (existingSkill != null)
        {
            return (
                $"Lv.{existingSkill.skillData.GetCurrentTypeStat().baseStat.skillLevel} → {existingSkill.skillData.GetCurrentTypeStat().baseStat.skillLevel + 1}",
                existingSkill.skillData.GetCurrentTypeStat()
            );
        }
        return ("New!", null);
    }

    private void OnSkillButtonClicked(SkillData skillData, Skill existingSkill)
    {
        try
        {
            if (existingSkill != null)
            {
                GameManager.Instance.player.AddOrUpgradeSkill(skillData);
                var updatedSkill = GameManager.Instance.player.skills.Find(s => s.skillData.ID == skillData.ID);
                skillSelectedCallback?.Invoke(updatedSkill);
            }
            else
            {
                if (GameManager.Instance.player.AddOrUpgradeSkill(skillData))
                {
                    var newSkill = GameManager.Instance.player.skills.Find(s => s.skillData.ID == skillData.ID);
                    skillSelectedCallback?.Invoke(newSkill);
                }
            }
            LevelUpPanelClose();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in skill selection: {e.Message}");
            ShowError("Failed to process skill selection");
        }
    }

    private void ShowNoSkillsAvailable()
    {
        ClearExistingButtons();
        if (maxLevelButtonPrefab != null)
        {
            var maxLevelButton = Instantiate(maxLevelButtonPrefab, list);
            maxLevelButton.Initialize(OnMaxLevelButtonClicked);
        }
    }

    private void OnMaxLevelButtonClicked()
    {
        LevelUpPanelClose();
    }

    private void ClearExistingButtons()
    {
        foreach (Transform child in list)
        {
            Destroy(child.gameObject);
        }
    }

    public void LevelUpPanelClose()
    {
        ClearExistingButtons();
        gameObject.SetActive(false);
    }

    private bool ValidateSkillData(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogError("Skill Data is null");
            return false;
        }

        if (skillData.Prefab == null)
        {
            Debug.LogError($"Missing prefab for skill: {skillData.Name}");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        if (errorPopup != null)
        {
            errorPopup.SetActive(true);
            if (errorText != null)
            {
                errorText.text = message;
            }
            StartCoroutine(HideErrorAfterDelay(3f));
        }
        Debug.LogError(message);
    }

    private IEnumerator HideErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorPopup != null)
        {
            errorPopup.SetActive(false);
        }
    }

    private void ShowElementalHeader(ElementType element)
    {
        Debug.Log($"Selected Element: {element}");
        // TODO: 실제 UI에 속성 헤더를 표시하는 로직 구현
    }
}
