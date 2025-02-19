using System.Collections;
using UnityEngine;

public abstract class BaseStateHandler : IGameStateHandler
{
    protected readonly GameManager Game;
    protected readonly GameLoopManager GameLoop;
    protected readonly PlayerUnitManager PlayerUnit;
    protected readonly UIManager UI;

    protected BaseStateHandler()
    {
        GameLoop = GameLoopManager.Instance;
        UI = UIManager.Instance;
        Game = GameManager.Instance;
        PlayerUnit = PlayerUnitManager.Instance;
    }

    public virtual void OnEnter()
    {
        if (UI != null)
        {
            UI.ClearUI();
        }
    }

    public virtual void OnExit()
    {
        SavePlayerState();
    }

    public virtual void OnFixedUpdate() { }

    public virtual void OnUpdate() { }

    protected virtual void SavePlayerState()
    {
        if (Game != null && Game.player != null)
        {
            if (Game.player.TryGetComponent<Inventory>(out var inventory))
            {
                var inventoryData = inventory.GetInventoryData();
                PlayerDataManager.Instance.SaveInventoryData(inventoryData);
            }
            if (PlayerUnit != null)
            {
                PlayerUnit.SaveGameState();
            }
        }
    }

    protected Coroutine StartCoroutine(IEnumerator routine)
    {
        return GameLoop.StartCoroutine(routine);
    }

    protected void StopCoroutine(IEnumerator routine)
    {
        GameLoop.StopCoroutine(routine);
    }
}
