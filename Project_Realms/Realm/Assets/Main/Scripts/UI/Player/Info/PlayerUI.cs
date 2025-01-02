using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private Player player;
    private OrbUI mpOrb;
    private OrbUI hpOrb;
    private Slider playerExp;
    private TextMeshProUGUI playerLevelText;
    public PlayerBarUI playerBarUI;
    public TooltipWindow tooltipWindow;
    public InventoryUI inventoryUI;
    public StatUI statUI;
    public SkillTreeUI skillTreeUI;
    public MonsterTargetUI monsterTargetUI;
    public PlayerPortrait playerPortrait;
    public MobGauge monsterGaugeUI;

    public void Initialize(Player player)
    {
        this.player = player;

        monsterGaugeUI = Instantiate(monsterGaugeUI, transform);
        monsterGaugeUI.Initialize();

        statUI = Instantiate(statUI, transform);
        statUI.Initialize(player);
        statUI.gameObject.SetActive(false);

        inventoryUI = Instantiate(inventoryUI, transform);
        inventoryUI.Initialize(player, this);

        skillTreeUI = Instantiate(skillTreeUI, transform);
        skillTreeUI.Initialize(player);
        skillTreeUI.gameObject.SetActive(false);

        playerBarUI = Instantiate(playerBarUI, transform);
        playerBarUI.Initialize(player);
        this.mpOrb = playerBarUI.mpOrb;
        this.hpOrb = playerBarUI.hpOrb;
        this.playerExp = playerBarUI.playerExpBar;

        monsterTargetUI = Instantiate(monsterTargetUI, transform);
        playerPortrait = Instantiate(playerPortrait, transform);
        this.playerLevelText = playerPortrait.playerLevelText;

        
        tooltipWindow = Instantiate(tooltipWindow, transform);
    }

    public void ShowSkillTreeUI()
    {
        skillTreeUI.gameObject.SetActive(true);
    }

    public void HideSkillTreeUI()
    {
        skillTreeUI.gameObject.SetActive(false);
    }

    public void ShowStatUI()
    {
        statUI.gameObject.SetActive(true);
        if (statUI != null)
            statUI.UpdateUI();
    }

    public void HideStatUI()
    {
        statUI.gameObject.SetActive(false);
    }

    public void UpdatePlayerLevel(int level)
    {
        playerLevelText.text = level.ToString();
    }

    public void UpdateOrbs(float manaAmount, float hpAmount)
    {
        hpOrb.ChangeOrbValue(hpAmount);
        mpOrb.ChangeOrbValue(manaAmount);
    }

    public void UpdatePlayerEXP(float amount)
    {
        playerExp.value = amount;
    }

    public void UpdatePlayerInfo()
    {
        float maxHealth = player.CharacterStats.GetStatValue(StatType.MaxHealth);
        float maxMana = player.CharacterStats.GetStatValue(StatType.MaxMana);

        float hp = maxHealth > 0 ? player.CharacterStats.GetStatValue(StatType.Health) / maxHealth : 0f;
        float mp = maxMana > 0 ? player.CharacterStats.GetStatValue(StatType.Mana) / maxMana : 0f;

        hp = Mathf.Clamp01(hp);
        mp = Mathf.Clamp01(mp);

        UpdateOrbs(mp, hp);

        float expAmount = player.ExpPercentage;

        int level = (int)player.CharacterStats.GetStatValue(StatType.Level);

        UpdatePlayerLevel(level);

        UpdatePlayerEXP(expAmount);
    }
}
