using UnityEngine;

public class GameOverStateHandler : BaseStateHandler
{
    private bool portalSpawned = false;

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Entering Game Over state");

        UI.ShowGameOverScreen();

        if (Game != null && Game.player != null)
        {
            PlayerUnit.SpawnPlayer(Vector3.zero);
            PlayerUnit.LoadGameState();
            Debug.Log("Player respawned at death location");
            CameraManager.Instance.SetupCamera(SceneType.Game);

            if (!portalSpawned)
            {
                SpawnTownPortal();
                portalSpawned = true;
            }
        }
    }

    public override void OnExit()
    {
        Debug.Log("Exiting Game Over state");
        UI.HideGameOverScreen();
        portalSpawned = false;

        if (Game != null && Game.player != null)
        {
            GameObject.Destroy(Game.player.gameObject);
            Game.player = null;
        }

        base.OnExit();
    }

    private void SpawnTownPortal()
    {
        if (Game != null && Game.player != null)
        {
            Vector3 playerPos = Game.player.transform.position;
            Vector3 portalPosition = playerPos + new Vector3(2f, 0f, 0f);
            StageManager.Instance.SpawnTownPortal(portalPosition);
            Debug.Log("Town portal spawned near player's death location");
        }
    }
}
