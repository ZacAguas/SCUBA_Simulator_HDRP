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

    private void FixedUpdate()
    {
        bcdSlider.value = playerController.GetNormalisedCurrentBCDVolume();
        depthLabel.text = "Depth: " + depthManager.Depth.ToString("000.00") + "m";
    }
}
