using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(TankController))] // for END
public class DepthManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private TankController tankController;
    private PlayerController playerController;
    private NitrogenNarcosisController nitrogenNarcosisController;
    
    public float Depth { get; private set; }
    public float MaxDepth => 300f;
    public float PressureAbsolute => (Depth + 10) / 10; // absolute atmospheric pressure ie. 1ATA at 0m, 2ATA at 10m

    [field: SerializeField] public float MaxAscentRate { get; private set; } // the max speed the player should ascend at (normally 9/16 m/min)
    public float CurrentAscentRate { get; private set; } // in m/min, positive if ascending, negative if descending
    
    

    private float prevDepth;
    private bool playerNarced = false;
    
    [SerializeField] private GameEvent onBecomeNarced; // triggered when player reaches certain depth determined by narcosisStartDepth
    [SerializeField] private GameEvent onEndNarced; // triggered when player reaches certain depth determined by narcosisStartDepth

    [SerializeField] private Transform seaLevel; // used for calculating depth
    [SerializeField] private float narcosisAirThreshold; // the depth at which narcosis starts (FOR AIR)
    private float equivalentNarcoticDepth; // estimate depth at which gas mixture produces equivalent narcotic effect to air

    private float narcosisLevel2Depth; // depth at which level 2 narcosis starts
    private float narcosisLevel3Depth; // depth at which level 3 narcosis starts

    private void Awake()
    {
        tankController = GetComponent<TankController>();
        playerController = GetComponent<PlayerController>();
        nitrogenNarcosisController = GetComponent<NitrogenNarcosisController>();
    }

    private void Start()
    {
        CalculateDepth();
        prevDepth = Depth;
        playerNarced = false;

        CalculateEquivalentNarcoticDepth(); // careful with script execution order (percentages must be calculated in tank controller before this)
        SetupNarcoticLevels();
    }
    private void FixedUpdate() // for physics calculations
    {
        CalculateDepth();
        CalculateEquivalentNarcoticDepth();
        CalculateAscentRate();
        CheckNarcosis();
        prevDepth = Depth; // keep track of the depth last frame
    }
    
    public float GetVolumeAtPressureATA(float volumeAtSurface)
    {
        return volumeAtSurface / PressureAbsolute;
    }

    private void SetupNarcoticLevels()
    {
        float rangePerLevel = (tankController.MOD - equivalentNarcoticDepth) / 3f;

        narcosisLevel2Depth = narcosisAirThreshold + rangePerLevel;
        narcosisLevel3Depth = narcosisAirThreshold + rangePerLevel * 2;
    }

    private void CalculateDepth()
    {
        Depth = Mathf.Max(0, seaLevel.position.y - transform.position.y);
    }

    private void CalculateEquivalentNarcoticDepth()
    {
        if (tankController.heliumPercentage > 0) // only for tri mix
            equivalentNarcoticDepth = (Depth + 10) * (1 - tankController.heliumPercentage) - 10; // formula: (depth+10)*(1-fraction of helium) - 10
        else
            equivalentNarcoticDepth = Depth;
    }

    private void CalculateAscentRate()
    {
        CurrentAscentRate = playerController.GetYVelocity() * 60; // * 60 because ascent rate is in m/min not m/s
    }
    

    private void CheckNarcosis()
    {
        if (equivalentNarcoticDepth >= narcosisAirThreshold) // raise narced event if past threshold and not already narced
        {
            // calculate narcotic level
            if (narcosisAirThreshold <= Depth && Depth <= narcosisLevel2Depth)
                nitrogenNarcosisController.NarcosisLevel = 1;
            else if (narcosisLevel2Depth <= Depth && Depth <= narcosisLevel3Depth)
                nitrogenNarcosisController.NarcosisLevel = 2;
            else if (narcosisLevel3Depth <= Depth && Depth <= tankController.MOD)
                nitrogenNarcosisController.NarcosisLevel = 3;
            else
            {
                nitrogenNarcosisController.NarcosisLevel = 4;
                gameManager.Die(CauseOfDeath.Depth, 3, true);
            }
            
            if (!playerNarced) // only invoke if not already narced
            {
                nitrogenNarcosisController.EnterNarcoticDepth();
            }
            playerNarced = true;
        }
        else
        {
            nitrogenNarcosisController.NarcosisLevel = 0;
            playerNarced = false;
        }
    }

    
}
