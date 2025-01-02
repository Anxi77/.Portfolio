using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBarUI : MonoBehaviour
{
    public SkillBarUI skillBarUI;
    public OrbUI hpOrb;
    public OrbUI mpOrb;
    public Slider playerExpBar;

    public void Initialize(Player player) 
    {
        skillBarUI.Initialize(player);        
    }
}
