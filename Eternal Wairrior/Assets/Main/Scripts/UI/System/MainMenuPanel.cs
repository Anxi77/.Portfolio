using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        InitializeButtons();
    }

    public void InitializeButtons()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => UIManager.Instance.OnStartNewGame());

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(() => UIManager.Instance.OnLoadGame());

        if (exitButton != null)
            exitButton.onClick.AddListener(() => UIManager.Instance.OnExitGame());
    }

    public void UpdateButtons(bool hasSaveData)
    {
        if (loadGameButton != null)
            loadGameButton.interactable = hasSaveData;
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveAllListeners();
        if (loadGameButton != null)
            loadGameButton.onClick.RemoveAllListeners();
        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
    }
}