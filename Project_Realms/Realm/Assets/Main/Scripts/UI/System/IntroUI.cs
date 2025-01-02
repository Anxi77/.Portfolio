using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(StartMainStage);
    }

    private void StartMainStage() 
    {
        SceneManager.LoadSceneAsync(1,LoadSceneMode.Single);
    }
}
