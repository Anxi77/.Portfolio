using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterTargetUI : MonoBehaviour
{
    private Slider monsterHPSlider;
    private TextMeshProUGUI monsterName;
    private Monster targetMonster;

    [SerializeField] private GameObject targetUI;

    private void Start()
    {
        Initialize();
        if (targetUI != null)
            targetUI.SetActive(false);
    }

    public void Initialize()
    {
        monsterHPSlider = GetComponentInChildren<Slider>();
        monsterName = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        SelectTarget();
        
        if (targetMonster != null)
        {
            UpdateTargetInfo();
        }
    }

    private void SelectTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Monster")))
        {

            targetMonster = hit.collider.GetComponent<Monster>();

            if (targetUI != null)
                targetUI.SetActive(true);

            UpdateTargetInfo();
        }
        else
        {
            targetMonster = null;
            if (targetUI != null)
                targetUI.SetActive(false);
        }
    }

    private void UpdateTargetInfo()
    {
        if (targetMonster == null)
            return;

        if (targetMonster.CharacterStats.GetStatValue(StatType.Health) <= 0)
        {
            targetMonster = null;
            if (targetUI != null)
                targetUI.SetActive(false);
            return;
        }

        float currentHealth = targetMonster.CharacterStats.GetStatValue(StatType.Health);
        float maxHealth = targetMonster.CharacterStats.GetStatValue(StatType.MaxHealth);
        monsterHPSlider.value = currentHealth / maxHealth;

        monsterName.text = targetMonster.name;
    }
}
