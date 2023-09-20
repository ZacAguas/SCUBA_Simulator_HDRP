using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private PlayerController playerController;
    private DepthManager depthManager;
    private InputManager inputManager; // for checking if inflating
    
    public float CurrentTankPressure {
        get => currentTankPressure;
        private set => currentTankPressure = Mathf.Max(0, value);
    }
    private float currentTankPressure; // backing field, should have no usages, use property instead

    [field: SerializeField] public float MaxTankPressure { get; private set; } // max pressure of tank in bar

    [field: SerializeField] public float MaxPPO2 { get; private set; }
    [field: SerializeField] public float MOD { get; private set; } // maximum operating depth of gas mixture

    // Air consumption fields
    public float Exertion
    {
        get => exertion;
        set => exertion = Mathf.Clamp01(value);
    } // multiplier used that increases air consumption, ranges from restingExertion to 1.0 (ie. 100%)
    private float exertion; // backing field
    [SerializeField] private float restingExertion; // default exertion value
    [SerializeField] private float cylinderVolume; // volume of cylinder in litres (normally 12 or 15)
    [SerializeField] private float sac; // average sac rate (15-18 litres/min for experienced divers)
    // BCD
    private float previousBCDVolume;
    

    public enum GasMix
    {
        Air,
        EAN32,
        EAN36,
        Tri21_35,
        Tri18_45,
        Tri10_70
    }
    [SerializeField] private GasMix selectedGasMix;
    public GasMix GetGasMix() => selectedGasMix;
    public float oxygenPercentage;
    public float nitrogenPercentage;
    public float heliumPercentage;

    [SerializeField] private float lowTankPercentageThreshold;  // point at which air in tank is considered 'low' eg. 0.33 = rule of thirds
    [SerializeField] private float tankUpdateInterval;  // how frequently the tank pressure is updated/checked (in seconds)
    public WaitForSeconds TankUpdateWaitForSeconds { get; private set; }
    

    [SerializeField] private GameEvent onPublishStats; // called every time the new tank pressure etc. is generated
    [SerializeField] private GameEvent onLowTankPressure; // triggered when tank reaches 'low' threshold
    [SerializeField] private GameEvent onOutOfAir; // triggered when tank reaches 0
    private bool isLowTankPressure;
    private bool isOutOfAir;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        depthManager = GetComponent<DepthManager>();
        inputManager = GetComponent<InputManager>();
        TankUpdateWaitForSeconds = new WaitForSeconds(tankUpdateInterval); // cache the wait for seconds based on the update interval
    }

    private void Start()
    {
        CurrentTankPressure = MaxTankPressure;
        Exertion = restingExertion;

        previousBCDVolume = playerController.CurrentBCDVolume;
        
        InitialiseGasMix();
        
        // Update/check pressure repeatedly
        StartCoroutine(TankPressure());
    }

    private void InitialiseGasMix()
    {
        switch (selectedGasMix)
        {
            case GasMix.Air:
                oxygenPercentage = 0.21f;
                nitrogenPercentage = 0.79f;
                heliumPercentage = 0f;
                break;
            case GasMix.EAN32:
                oxygenPercentage = 0.32f;
                nitrogenPercentage = 0.68f;
                heliumPercentage = 0f;
                break;
            case GasMix.EAN36:
                oxygenPercentage = 0.36f;
                nitrogenPercentage = 0.64f;
                heliumPercentage = 0f;
                break;
            case GasMix.Tri21_35:
                oxygenPercentage = 0.21f;
                nitrogenPercentage = 0.44f;
                heliumPercentage = 0.35f;
                break;
            case GasMix.Tri18_45:
                oxygenPercentage = 0.18f;
                nitrogenPercentage = 0.37f;
                heliumPercentage = 0.45f;
                break;
            case GasMix.Tri10_70:
                oxygenPercentage = 0.10f;
                nitrogenPercentage = 0.2f;
                heliumPercentage = 0.7f;
                break;
        }

        MOD = (MaxPPO2 / oxygenPercentage - 1) * 10; // calculate Maximum Operating Depth
    }

    private void CheckTankPressure()
    {
        if (!isLowTankPressure && CurrentTankPressure <= MaxTankPressure * lowTankPercentageThreshold) // we have 'low' pressure for the first time
        {
            onLowTankPressure.Invoke(); // raise low pressure event
            isLowTankPressure = true;
        }

        if (!isOutOfAir && CurrentTankPressure == 0) // we are out of air for the first time
        {
            isOutOfAir = true;
            gameManager.Die(CauseOfDeath.OutOfAir, 5, false);
        }
    }
    private void UpdateTankPressure()
    {
        // reduce tank based on exertion and air consumption rate
        CurrentTankPressure -= CalculateConsumption();
     
        // reduce tank based on BCD inflate volume
        CurrentTankPressure -= CalculateInflateConsumption();
    }
    private float CalculateConsumption()
    {
        // NOTE: ATA not to be confused with ATM, ATA is ATM+1 because it takes into account the ambient pressure at sea level
        float intervalAdjustedSAC = (sac / 60) * tankUpdateInterval; // SAC adjusted for update interval in secs instead of per minute
        float exertionAdjustedSAC;
        if (restingExertion != 0)
            exertionAdjustedSAC = (intervalAdjustedSAC / restingExertion) * Exertion; // surface air consumption adjusted for exertion multiplier
        else
            exertionAdjustedSAC = 0;

        float gasConsumed = exertionAdjustedSAC * depthManager.PressureAbsolute;
        float pressureUsed = gasConsumed / cylinderVolume;
        return pressureUsed;
    }

    private float CalculateInflateConsumption()
    {
        float pressureUsed = 0;
        float currentBCDVolume = playerController.CurrentBCDVolume;

        if (inputManager.GetBCDInput() > 0) // inflating
        {
            // calculate the air consumed per update interval only when inflating
            float volumeDifferenceInterval = (currentBCDVolume - previousBCDVolume) * tankUpdateInterval; // difference per time interval

            float gasConsumed = Mathf.Max(volumeDifferenceInterval, 0) * depthManager.PressureAbsolute; // ensure only consume gas when difference is positive (inflating not deflating)
            pressureUsed = gasConsumed / cylinderVolume;


            
        }
        
        previousBCDVolume = currentBCDVolume; // for next iteration
        return pressureUsed;
    }

    private IEnumerator TankPressure()
    {
        while (true)
        {
            UpdateTankPressure();
            CheckTankPressure();
            yield return TankUpdateWaitForSeconds;
        }
    }
}
