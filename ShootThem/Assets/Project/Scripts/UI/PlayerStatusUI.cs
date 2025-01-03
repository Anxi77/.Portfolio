using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    public Slider healthSlider;
    public Slider manaSlider;
    public TextMeshProUGUI levelText;
    public Player player;

    private void Update()
    {
        healthSlider.value = player.CurrentHealth;
        manaSlider.value = player.CurrentMana;
        levelText.text = "Lv. " + player.CurrentLevel.ToString();
    }
}