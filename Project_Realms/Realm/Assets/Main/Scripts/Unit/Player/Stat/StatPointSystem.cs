using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerStat))]
public class StatPointSystem : MonoBehaviour
{
    private Player owner;
    private PlayerStat playerStat;
    public int AvailablePoints { get; private set; }

    public void Initialize(Player player)
    {
        owner = player;
        playerStat = (PlayerStat)player.CharacterStats;
    }

    public void AddStatPoints(int points)
    {
        AvailablePoints += points;
    }

    public bool TryInvestPoint(StatType statType)
    {
        if (AvailablePoints <= 0) return false;

        Stat stat = playerStat.GetStat(statType);
        if (stat == null) return false;

        float increaseAmount = playerStat.GetPointIncreaseAmount(statType);
        if (increaseAmount <= 0) return false; 

        stat.InvestPoint(increaseAmount);
        owner.UpdateMoveSpeed();
        AvailablePoints--;
        return true;
    }
}