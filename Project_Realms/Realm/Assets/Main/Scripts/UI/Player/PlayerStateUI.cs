using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStateUI : MonoBehaviour
{
    private Player player;
    private PlayerStateHandler psh;
    private TextMeshProUGUI playerStateText;

    public void Initialize(Player player) 
    {
        this.player = player;
        psh = player.PlayerHandler;
        playerStateText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (playerStateText != null) 
        {
            GetPlayerState();
        }
    }

    private void GetPlayerState() 
    {
        playerStateText.text = psh.CurrentState.ToString();
    }


}
