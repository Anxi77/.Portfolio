using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StatUI : MonoBehaviour
{
    [System.Serializable]
    public class StatDisplay
    {
        public StatType statType;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI valueText;
        public Button increaseButton;
    }

    [SerializeField] private StatDisplay[] statDisplays;
    [SerializeField] private TextMeshProUGUI availablePointsText;

    private Player player;
    private PlayerStat unitStats;
    private StatPointSystem statPointSystem;

    public void Initialize(Player player)
    {
        this.player = player;
        unitStats = (PlayerStat)player.CharacterStats;
        statPointSystem = player.statPointSystem;
        
        InitializeUI();
    }

    private void InitializeUI()
    {
        foreach (var display in statDisplays)
        {
            if (display.increaseButton != null)
            {
                StatType statType = display.statType;
                display.increaseButton.onClick.AddListener(() => OnStatIncreaseButton(statType));
            }
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (availablePointsText != null)
        {
            availablePointsText.text = $"{statPointSystem.AvailablePoints}";
        }

        bool hasAvailablePoints = statPointSystem.AvailablePoints > 0;

        foreach (var display in statDisplays)
        {
            if (display.statType == StatType.Level)
            {
                int level = (int)unitStats.GetStatValue(display.statType);
                display.valueText.text = level.ToString();
            }
            else 
            {
                float value = unitStats.GetStatValue(display.statType);
                display.valueText.text = value.ToString("F1");
            }


            if (display.increaseButton != null)
            {
                bool canInvest = hasAvailablePoints &&
                                unitStats.GetPointIncreaseAmount(display.statType) > 0;

                display.increaseButton.gameObject.SetActive(canInvest);
            }
        }
    }

    private void OnStatIncreaseButton(StatType statType)
    {
        if (statPointSystem.TryInvestPoint(statType))
        {
            UpdateUI();
        }
    }
}