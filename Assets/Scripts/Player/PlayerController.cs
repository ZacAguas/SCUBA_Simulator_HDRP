using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float totalMass => bodyMass + equipmentMass;
    [SerializeField] private float bodyMass; // in kg
    [SerializeField] private float equipmentMass; // in kg
    private float bodyVolume; // in cubic meters
    // BCD
    [SerializeField] private float maxSurfaceBCDVolume; // in m^3
    [SerializeField] private float inflateSpeed;

    private float normalisedCurrentBCDVolume; // 0 = fully deflated, 1 = fully inflated
    public float GetNormalisedCurrentBCDVolume() => normalisedCurrentBCDVolume;
    
    public float CurrentBCDVolume
    {
        get
        {
            float volumeAtCurrentPressure = depthManager.GetVolumeAtPressureATA(normalisedCurrentBCDVolume * maxSurfaceBCDVolume); // the current volume of the BCD at this depth/pressure
            float maxVolumeAtCurrentPressure = depthManager.GetVolumeAtPressureATA(maxSurfaceBCDVolume); // the volume if the BCD is fully inflated at this pressure
            return Mathf.Min(volumeAtCurrentPressure, maxVolumeAtCurrentPressure); // ensures volume doesn't go over maximum
        }
    }

    private float currentBCDVolume; // backing field, don't use

    private float yMouseRotation;
    private float xMouseRotation;
    
    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        depthManager = GetComponent<DepthManager>();
        rb = GetComponent<Rigidbody>();


        rb.mass = bodyMass;
        bodyVolume = CalculateBodyVolume();
        normalisedCurrentBCDVolume = 1; // start fully inflated
        Debug.Log("Body volume: " + bodyVolume);
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


    private void MouseMovement()
    {
        Vector2 mouseInput = inputManager.GetMouseInput();
        yMouseRotation += mouseInput.x * mouseSensitivity * Time.deltaTime;
        xMouseRotation -= mouseInput.y * mouseSensitivity * Time.deltaTime;
        
        if (clampVerticalLook)
            xMouseRotation = Mathf.Clamp(xMouseRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xMouseRotation, yMouseRotation, 0);
    }

    private void AdjustBCD()
    {
        float input = inputManager.GetBCDInput();

        float targetNormalisedVolume = normalisedCurrentBCDVolume + input * inflateSpeed * Time.fixedDeltaTime; // target is unbound ie. may be outside range 0-1
        normalisedCurrentBCDVolume = Mathf.Clamp01(targetNormalisedVolume); // ensures value is normalised
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
