using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkillLevelUpPanel : MonoBehaviour
{
    public RectTransform list;
    public SkillLevelUpButton buttonPrefab;
    private const int SKILL_CHOICES = 3;

    public void LevelUpPanelOpen(List<Skill> playerSkills, Action<Skill> callback)
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        // ��� ������ ��� ��ų ������ ��������
        var allSkillData = SkillDataManager.Instance.GetAllSkillData();

        // ���� �Ӽ� ���� (None ����)
        var availableElements = Enum.GetValues(typeof(ElementType))
            .Cast<ElementType>()
            .Where(e => e != ElementType.None)
            .ToList();

        if (availableElements.Count == 0)
        {
            ShowNoSkillsAvailable();
            return;
        }

        ElementType selectedElement = availableElements[UnityEngine.Random.Range(0, availableElements.Count)];

        // ���õ� �Ӽ��� ��ų�� ���͸�
        var elementalSkills = allSkillData
            .Where(skill =>
            {
                var stats = skill.GetCurrentTypeStat();
                return stats.baseStat.element == selectedElement;
            })
            .ToList();

        // �̹� ������ ��ų ����
        elementalSkills = elementalSkills
            .Where(skillData => !playerSkills.Any(playerSkill => playerSkill.SkillID == skillData._SkillID))
            .ToList();

        if (elementalSkills.Count == 0)
        {
            ShowNoSkillsAvailable();
            return;
        }

        // �����ϰ� 3�� ���� (�Ǵ� ������ ��ŭ)
        int choiceCount = Mathf.Min(SKILL_CHOICES, elementalSkills.Count);
        List<SkillData> selectedSkills = new List<SkillData>();

        while (selectedSkills.Count < choiceCount)
        {
            int randomIndex = UnityEngine.Random.Range(0, elementalSkills.Count);
            var selectedSkill = elementalSkills[randomIndex];

            if (!selectedSkills.Contains(selectedSkill))
            {
                selectedSkills.Add(selectedSkill);
                CreateSkillButton(selectedSkill, callback);
            }
        }

        // ���õ� �Ӽ� ǥ��
        ShowElementalHeader(selectedElement);
    }

    private void CreateSkillButton(SkillData skillData, Action<Skill> callback)
    {
        SkillLevelUpButton skillButton = Instantiate(buttonPrefab, list);

        skillButton.SetSkillSelectButton(skillData, () =>
        {
            // ��ų ������ ���� �� �ʱ�ȭ
            GameObject skillObj = Instantiate(skillData.prefabsByLevel[0], GameManager.Instance.player.transform);
            if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
            {
                callback(newSkill);
            }
            LevelUpPanelClose();
        });
    }

    private string GetElementalDescription(ElementType element)
    {
        return element switch
        {
            ElementType.Dark => "��� �Ӽ�: ���� ����",
            ElementType.Water => "�� �Ӽ�: �̵��ӵ� ����",
            ElementType.Fire => "�� �Ӽ�: ���� ������",
            ElementType.Earth => "���� �Ӽ�: ����",
            _ => ""
        };
    }

    private void ShowElementalHeader(ElementType element)
    {
        string elementName = element switch
        {
            ElementType.Dark => "���",
            ElementType.Water => "��",
            ElementType.Fire => "��",
            ElementType.Earth => "����",
            _ => "�� �� ����"
        };

        // ��� UI ���� �� ���� (UI ������Ʈ�� �°� ���� �ʿ�)
        Debug.Log($"���õ� �Ӽ�: {elementName}");
    }

    private void ShowNoSkillsAvailable()
    {
        SkillLevelUpButton skillButton = Instantiate(buttonPrefab, list);
        skillButton.SetDisabledButton("��� ������ ��ų ����");
    }

    public void LevelUpPanelClose()
    {
        foreach (Transform button in list)
        {
            Destroy(button.gameObject);
        }
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
