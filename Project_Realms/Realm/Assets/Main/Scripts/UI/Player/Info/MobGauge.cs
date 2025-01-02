using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobGauge : MonoBehaviour
{
    private Slider mobGaugeSlider;
    private bool isInitialized = false;

    public void Initialize() 
    {
        StartCoroutine(InitializeRoutine());
    }

    public IEnumerator InitializeRoutine() 
    {
        yield return new WaitUntil(() => MonsterManager.Instance != null && MonsterManager.Instance.isInitialized);
        mobGaugeSlider = GetComponentInChildren<Slider>();
        isInitialized = true;
    }

    private void Update()
    {
        if (mobGaugeSlider != null && isInitialized) 
        {
            mobGaugeSlider.value = MonsterManager.Instance.GaugePercentage;
        }
    }
}
