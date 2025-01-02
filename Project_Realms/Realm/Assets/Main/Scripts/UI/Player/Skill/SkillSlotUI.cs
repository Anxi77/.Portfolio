using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI hotkeyText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Button button;
    private SkillSelectUI skillSelectUI;

    private Skill currentSkill;
    private KeyCode hotkey;
    private RectTransform rectTransform;

    public void SetSkill(Skill skill, KeyCode key)
    {
        currentSkill = skill;
        hotkey = key;

        if (skill != null && skill.data != null)
        {
            skillIcon.sprite = skill.data.skillIcon;
            skillIcon.enabled = true;
            skillIcon.color = Color.white;
            if(hotkeyText != null) { hotkeyText.text = key.ToString(); }            
        }
        else
        {
            skillIcon.enabled = false;
            if (hotkeyText != null) { hotkeyText.text = key.ToString(); }
            cooldownText.text = "";
        }
    }

    public void SetSkillSelectUI(SkillSelectUI selectUI)
    {
        this.skillSelectUI = selectUI;
    }

    private void Start()
    {
        button.onClick.AddListener(OnSlotClicked);
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnSlotClicked()
    {
        if(hotkey != KeyCode.Mouse0) 
        {
            skillSelectUI.ShowSkillSelect(hotkey, rectTransform);
        }
    }

    private void Update()
    {
        if (currentSkill == null) return;

        UpdateCooldown();
    }

    private void UpdateCooldown()
    {
        if (currentSkill.IsOnCooldown)
        {
            float cooldownRatio = currentSkill.RemainingCooldown / currentSkill.TotalCooldown;
            Color iconColor = Color.Lerp(Color.gray, Color.white, 1 - cooldownRatio);
            skillIcon.color = iconColor;
            cooldownText.text = Mathf.Ceil(currentSkill.RemainingCooldown).ToString();
        }
        else
        {
            skillIcon.color = Color.white;
            cooldownText.text = "";
        }
    }
}