using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(TankController))] // for END
public class DepthManager : MonoBehaviour
{
    private TankController tankController;
    
    public float Depth { get; private set; }
    public float PressureAbsolute => (Depth + 10) / 10; // absolute atmospheric pressure ie. 1ATA at 0m, 2ATA at 10m

    private float prevDepth;
    private bool playerNarced = false;
    
    [SerializeField] private GameEvent onBecomeNarced; // triggered when player reaches certain depth determined by narcosisStartDepth

    [SerializeField] private Transform seaLevel;
    [SerializeField] private float narcosisAirThreshold; // the depth at which narcosis starts (FOR AIR)
    private float equivalentNarcoticDepth; // estimate depth at which gas mixture produces equivalent narcotic effect to air

    private void Awake()
    {
        tankController = GetComponent<TankController>();
    }

    private void Start()
    {
        CalculateDepth();
        prevDepth = Depth;
        playerNarced = false;

        CalculateEquivalentNarcoticDepth(); // careful with script execution order (percentages must be calculated in tank controller before this)
        
        Debug.Log("Start depth: " + Depth);
    }
    private void FixedUpdate() // for physics calculations
    {
        CalculateDepth();
        CalculateEquivalentNarcoticDepth();
        CheckNarcosis();


        prevDepth = Depth; // keep track of the depth last frame
    }
    
    public float GetVolumeAtPressureATA(float volumeAtSurface)
    {
        return volumeAtSurface / PressureAbsolute;
    }

    private void CalculateDepth()
    {
        Depth = seaLevel.position.y - transform.position.y;
    }

    private void CalculateEquivalentNarcoticDepth()
    {
        if (tankController.heliumPercentage > 0) // only for tri mix
            equivalentNarcoticDepth = (Depth + 10) * (1 - tankController.heliumPercentage) - 10; // formula: (depth+10)*(1-fraction of helium) - 10
        else
            equivalentNarcoticDepth = Depth;
    }

    private void CheckNarcosis()
    {
        if (equivalentNarcoticDepth >= narcosisAirThreshold) // raise narced event if past threshold and not already narced
        {
            if (!playerNarced) // only invoke if not already narced
            {
                onBecomeNarced.Invoke(); // do I need to check if this is null?
                Debug.Log("invoke narced event");
            }
            playerNarced = true;
        }
        else
        {
            playerNarced = false;
        }
    }

    
}
