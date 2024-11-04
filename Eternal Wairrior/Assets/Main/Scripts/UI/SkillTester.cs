using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTester : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown skillDropdown;
    [SerializeField] private Button addSkillButton;

    private void Start()
    {
        InitializeDropdown();
        SetupButton();
    }

    private void InitializeDropdown()
    {
        if (skillDropdown == null) return;

        skillDropdown.ClearOptions();
        var skillDatas = SkillDataManager.Instance.GetAllSkillData();

        foreach (var skillData in skillDatas)
        {
            skillDropdown.options.Add(new TMP_Dropdown.OptionData(
                $"{skillData.metadata.Name} ({skillData.metadata.Type})"
            ));
        }

        skillDropdown.RefreshShownValue();
    }

    private void SetupButton()
    {
        if (addSkillButton == null) return;

        addSkillButton.onClick.AddListener(() => {
            if (GameManager.Instance?.player == null) return;

            var skillDatas = SkillDataManager.Instance.GetAllSkillData();
            if (skillDropdown.value < skillDatas.Count)
            {
                var selectedSkill = skillDatas[skillDropdown.value];
                GameManager.Instance.player.AddOrUpgradeSkill(selectedSkill);
                Debug.Log($"Added/Upgraded skill: {selectedSkill.metadata.Name}");
            }
        });
    }

    // ����Ű�� ��ų �߰� (���û���)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && skillDropdown != null)  // TŰ�� ������ ���� ���õ� ��ų �߰�
        {
            addSkillButton.onClick.Invoke();
        }
    }
}