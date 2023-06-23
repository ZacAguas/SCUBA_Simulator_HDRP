using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DepthManager depthManager;
    
    [SerializeField] private Slider bcdSlider;
    [SerializeField] private TextMeshProUGUI depthLabel;
    [SerializeField] private TextMeshProUGUI ascentLabel;
    

    private void FixedUpdate()
    {
        bcdSlider.value = playerController.GetInflationMultiplier();
        depthLabel.text = "Depth: " + depthManager.Depth.ToString("000.00") + "m";


        float currentAscentRate = depthManager.CurrentAscentRate;
        float maxAscentRate = depthManager.MaxAscentRate;
        ascentLabel.color = Color.white;

        if (currentAscentRate < 0) // descending
            ascentLabel.text = "DESC: " + Mathf.Abs(currentAscentRate).ToString("000.0" + "m/min");
        else // ascending
        {
            if (currentAscentRate > maxAscentRate) // ascending too quickly
                ascentLabel.color = Color.red;
            ascentLabel.text = "ASCN: " + currentAscentRate.ToString("000.0" + "m/min");

        }
    }
}
