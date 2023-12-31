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
    [SerializeField] private TankController tankController;
    [SerializeField] private NitrogenNarcosisController nitrogenNarcosisController;
    
    [SerializeField] private Timer timer;
    
    [SerializeField] private Slider bcdSlider;
    [SerializeField] private TextMeshProUGUI depthValue;
    [SerializeField] private TextMeshProUGUI ascentTitle;
    [SerializeField] private TextMeshProUGUI ascentValue;
    [SerializeField] private TextMeshProUGUI gasMix;
    [SerializeField] private TextMeshProUGUI maxDepthValue;
    [SerializeField] private TextMeshProUGUI timeValue;

    [SerializeField] private float UpdateInterval;

    private void Start()
    {
        // gas mix UI
        string gasText = tankController.GetGasMix().ToString();
        gasText = gasText.Replace('_', '/');
        gasMix.text = gasText;

        maxDepthValue.text = tankController.MOD.ToString(depthManager.Depth < 100 ? "00.0" : "000.0");
        
        
        StartCoroutine(UpdateUI());
    }

    private IEnumerator UpdateUI()
    {
        if (UpdateInterval == 0)
        {
            Debug.LogWarning("Update interval of UI must be more than 0");
            yield break;
        }

        WaitForSeconds interval = new WaitForSeconds(UpdateInterval);

        while (true)
        {
            bcdSlider.value = playerController.GetInflationMultiplier();
            
            // depth value colour
            switch (nitrogenNarcosisController.NarcosisLevel)
            {
                case 2:
                case 3:
                    depthValue.color = Color.yellow;
                    break;
                case 4: // player past MOD
                    depthValue.color = Color.red;
                    break;
                default: // player is not narced/slightly narced
                    depthValue.color = Color.white;
                    break;
            }
            
            depthValue.text = depthManager.Depth.ToString(depthManager.Depth < 100 ? "00.0" : "000.0");
            
            UpdateAscentRate();


            timeValue.text = timer.TimeInMMSS();
            yield return interval;
        }
        
        
    }

    private void UpdateAscentRate()
    {
        
        float currentAscentRate = depthManager.CurrentAscentRate;
        float maxAscentRate = depthManager.MaxAscentRate;
        ascentValue.color = Color.white;

        if (currentAscentRate < 0) // descending
            ascentTitle.text = "DESC:";
        else // ascending
        {
            ascentTitle.text = "ASCN:";
            if (currentAscentRate > maxAscentRate) // ascending too quickly
                ascentValue.color = Color.red;
        }
        ascentValue.text = Mathf.Abs(currentAscentRate).ToString(currentAscentRate < 100 ? "00.0" : "000.0");
    }
}
