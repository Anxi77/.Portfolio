using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SkillLevelUpButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image elementIcon;  // �Ӽ� ������
    [SerializeField] private Button button;

    public void SetSkillSelectButton(SkillData skillData, Action onClick)
    {
        // ��ų ������ ����
        if (skillIcon != null)
        {
            skillIcon.sprite = skillData.icon?.sprite;
            skillIcon.gameObject.SetActive(skillData.icon != null);
        }

        // ��ų �̸� ����
        if (skillNameText != null)
        {
            skillNameText.text = skillData.Name;
        }

        // ��ų ���� ����
        if (descriptionText != null)
        {
            string elementDesc = GetElementalDescription(skillData.GetCurrentTypeStat().baseStat.element);
            descriptionText.text = $"{skillData.Description}\n{elementDesc}";
        }

        // �Ӽ� ������ ����
        if (elementIcon != null)
        {
            elementIcon.sprite = GetElementSprite(skillData.GetCurrentTypeStat().baseStat.element);
            elementIcon.gameObject.SetActive(skillData.GetCurrentTypeStat().baseStat.element != ElementType.None);
        }

        // ��ư Ŭ�� �̺�Ʈ ����
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    // ��Ȱ��ȭ�� ��ư��
    public void SetDisabledButton(string message)
    {
        if (skillIcon != null) skillIcon.gameObject.SetActive(false);
        if (elementIcon != null) elementIcon.gameObject.SetActive(false);
        if (skillNameText != null) skillNameText.text = message;
        if (descriptionText != null) descriptionText.text = "";
        if (button != null) button.interactable = false;
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

    private Sprite GetElementSprite(ElementType element)
    {
        // ���ҽ����� �Ӽ� ������ �ε�
        string iconPath = element switch
        {
            ElementType.Dark => "Icons/DarkElement",
            ElementType.Water => "Icons/WaterElement",
            ElementType.Fire => "Icons/FireElement",
            ElementType.Earth => "Icons/EarthElement",
            _ => ""
        };
        return !string.IsNullOrEmpty(iconPath) ? Resources.Load<Sprite>(iconPath) : null;
    }
}