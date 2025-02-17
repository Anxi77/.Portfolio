using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerSkillIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image elementalBorder;
    private Skill skill;

    public void SetSkillIcon(Sprite iconSprite, Skill skill)
    {
        try
        {
            this.skill = skill;

            if (iconImage != null)
            {
                if (iconSprite == null)
                {
                    iconSprite = Resources.Load<Sprite>("DefaultSkillIcon");
                    Debug.LogWarning($"Using default icon for skill: {skill?.GetType().Name}");
                }
                iconImage.sprite = iconSprite;
                iconImage.gameObject.SetActive(iconSprite != null);
            }
            else
            {
                Debug.LogError("Icon Image component is missing!");
            }

            if (levelText != null && skill != null)
            {
                levelText.text = $"Lv.{skill.currentLevel}";
                levelText.gameObject.SetActive(true);
            }

            if (elementalBorder != null && skill != null)
            {
                ElementType element = skill.GetSkillData()?.Element ?? ElementType.None;
                elementalBorder.color = GetElementColor(element);
                elementalBorder.gameObject.SetActive(element != ElementType.None);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting skill icon: {e.Message}");
        }
    }

    private Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => new Color(1f, 0.3f, 0.3f, 0.5f),
            ElementType.Water => new Color(0.3f, 0.3f, 1f, 0.5f),
            ElementType.Earth => new Color(0.3f, 0.8f, 0.3f, 0.5f),
            ElementType.Dark => new Color(0.5f, 0.2f, 0.7f, 0.5f),
            _ => new Color(1f, 1f, 1f, 0f)
        };
    }

    private void OnValidate()
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (levelText == null)
            levelText = GetComponentInChildren<TextMeshProUGUI>();

        if (elementalBorder == null)
            elementalBorder = transform.Find("ElementalBorder")?.GetComponent<Image>();
    }
}

