using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(DepthManager))]
public class PlayerController : MonoBehaviour
{
    private InputManager inputManager;
    private DepthManager depthManager;
    private Rigidbody rb;
    
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private bool clampVerticalLook;
    [SerializeField] private float swimSpeed;

    // world
    private const float waterDensity = 1030; // in kg/m^3
    private const float gravity = 9.81f; // acceleration due to gravity in m/s^2
    // body
    private float totalMass => bodyMass + equipmentMass;
    [SerializeField] private float bodyMass; // in kg
    [SerializeField] private float equipmentMass; // in kg
    private float bodyVolume; // in cubic meters
    // BCD
    [SerializeField] private float maxSurfaceBCDVolume; // in m^3
    [SerializeField] private float inflateSpeed;

    private float inflationMultiplier; // 0 = fully deflated, 1 = fully inflated
    public float GetInflationMultiplier() => inflationMultiplier;
    public float GetYVelocity() => rb.velocity.y;
    public float CurrentBCDVolume
    {
        get
        {
            float volumeAtCurrentPressure = depthManager.GetVolumeAtPressureATA(inflationMultiplier * maxSurfaceBCDVolume); // the current volume of the BCD at this depth/pressure
            float maxVolumeAtCurrentPressure = depthManager.GetVolumeAtPressureATA(maxSurfaceBCDVolume); // the volume if the BCD is fully inflated at this pressure
            return Mathf.Min(volumeAtCurrentPressure, maxVolumeAtCurrentPressure); // ensures volume doesn't go over maximum
        }
    }

    private float yMouseRotation;
    private float xMouseRotation;
    
    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        depthManager = GetComponent<DepthManager>();
        rb = GetComponent<Rigidbody>();


        rb.mass = bodyMass;
        bodyVolume = CalculateBodyVolume();
        inflationMultiplier = 1; // start fully inflated
    }

    private void Start()
    {
    }

    private void Update()
    {
        MouseMovement();
    }

    private void FixedUpdate()
    {
        SwimMovement();
        AdjustBCD();
        ApplyForces();
    }


    private void  MouseMovement()
    {
        Vector2 mouseInput = inputManager.GetMouseInput();
        
        float verticalRotationAngle = transform.localRotation.eulerAngles.x;
        float invertedXInput;
        if (verticalRotationAngle is > 90f and < 270f) {
            invertedXInput = -mouseInput.x; // Invert X if the camera is upside down
        } else {
            invertedXInput = mouseInput.x; // Don't invert X if the camera is not upside down
        }
        
        
        yMouseRotation += invertedXInput * mouseSensitivity * Time.deltaTime;
        xMouseRotation -= mouseInput.y * mouseSensitivity * Time.deltaTime;

        if (clampVerticalLook)
            xMouseRotation = Mathf.Clamp(xMouseRotation, -90f, 90f);

        // transform.localRotation = Quaternion.Euler(xMouseRotation, yMouseRotation, 0);
        rb.MoveRotation(Quaternion.Euler(xMouseRotation, yMouseRotation, 0));
    }

    private void AdjustBCD()
    {
        float input = inputManager.GetBCDInput();

        float targetNormalisedVolume = inflationMultiplier + input * inflateSpeed * Time.fixedDeltaTime; // target is unbound ie. may be outside range 0-1
        inflationMultiplier = Mathf.Clamp01(targetNormalisedVolume); // ensures value is normalised
    }
    
    private void ApplyForces()
    {
        float totalVolume = CurrentBCDVolume + bodyVolume;
        float buoyantForce = waterDensity * totalVolume * gravity;

        rb.AddForce(Vector3.up * (buoyantForce - (totalMass * gravity))); // buoyant force acting upwards, weight acting downwards
    }

    private void SwimMovement()
    {
        Vector3 swimInput = inputManager.GetSwimInput();
        
        rb.AddRelativeForce(swimInput * swimSpeed); // relative force applies the force according to local rotation
    }

    private float CalculateBodyVolume()
    {
        // note: this is an estimate
        return bodyMass / 1000f; // in cubic meters
    }

    
    
}
