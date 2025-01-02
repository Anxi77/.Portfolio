using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeSlot : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;
    private Skill skill;

    public void Initialize(Skill skill)
    {
        this.skill = skill;

        if (this.skill != null && this.skill.data != null)
        {
            skillIcon.sprite = this.skill.data.skillIcon;
        }

        button.onClick.AddListener(OnButtonClicked);
        UpdateLevel(0);
    }

    public void UpdateLevel(int level)
    {
        levelText.text = level.ToString();
    }

    private void OnButtonClicked()
    {
        GetComponentInParent<SkillTreeUI>()?.OnSkillSlotClicked(skill);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }
}