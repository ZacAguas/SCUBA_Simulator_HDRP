using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GearController : MonoBehaviour
{
    [SerializeField] private DepthManager depthManager;
    [SerializeField] private InputManager inputManager;

    [SerializeField] private Renderer lcdRenderer;
    private Material lcdMaterial;
    [SerializeField] private float defaultLCDIntensity;
    [SerializeField] private float focusedLCDIntensity;
    
    // gear sway
    [SerializeField] private float swayMultiplier;
    [SerializeField] private float swaySmoothing;
    
    

    // equipment focusing
    private bool isFocusedLeft; // true if player is currently focused on left gear item
    private bool isFocusedRight;
    
    [SerializeField] private float transitionDuration;
    
    [SerializeField] private Transform currentLeftTransform; // transform of left gear item
    [SerializeField] private Transform defaultLeftTransform; // unfocused position
    [SerializeField] private Transform focusedLeftTransform; // focused position
    [SerializeField] private Transform currentRightTransform;
    [SerializeField] private Transform defaultRightTransform;
    [SerializeField] private Transform focusedRightTransform;
    private static readonly int IntensityPropertyID = Shader.PropertyToID("Intensity");

    private void Start()
    {
        lcdMaterial = lcdRenderer.material;
        lcdMaterial.SetFloat(IntensityPropertyID, defaultLCDIntensity);
    }

    private void Update()
    {
        FocusEquipment();
        SwayGear();
    }

    private void SwayGear()
    {
        float swayMultiplierThisFrame = swayMultiplier; // depends on focus
        if (isFocusedLeft || isFocusedRight)
            swayMultiplierThisFrame = swayMultiplier / 4; // sway less when focusing
        
        Vector2 mouseInput = inputManager.GetMouseInput() * swayMultiplierThisFrame;
        Quaternion rotationX = Quaternion.AngleAxis(-mouseInput.y, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseInput.x, Vector3.up);
        Quaternion targetRotation = rotationX * rotationY;
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, swaySmoothing);
        
    }
    
    private void FocusEquipment()
    {
        // Left click & not currently focused on other side
        if (Input.GetMouseButtonDown(0) && !isFocusedRight)
        {
            HandleLCDIntensity();
            HandleFocus(isFocusedLeft, currentLeftTransform, defaultLeftTransform, focusedLeftTransform);
            isFocusedLeft = !isFocusedLeft;
        }
        
        else if (Input.GetMouseButtonDown(1) && !isFocusedLeft)
        {
            HandleFocus(isFocusedRight, currentRightTransform, defaultRightTransform, focusedRightTransform);
            isFocusedRight = !isFocusedRight;
        }
    }

    private void HandleFocus(bool isFocused, Transform currentTransform, Transform defaultTransform, Transform focusedTransform)
    {
        Transform targetTransform = isFocused ? defaultTransform : focusedTransform; // target = default if focused
        
        currentTransform.DOLocalMove(targetTransform.localPosition, transitionDuration);
        currentTransform.DOLocalRotate(targetTransform.localRotation.eulerAngles, transitionDuration);
    }

    private void HandleLCDIntensity()
    {
        float normalisedDepth = depthManager.Depth / depthManager.MaxDepth;
        float depthFocusedLCDIntensity = Mathf.Lerp(defaultLCDIntensity, focusedLCDIntensity, normalisedDepth); // affected by depth

        float from = lcdMaterial.GetFloat(IntensityPropertyID);
        float to = isFocusedLeft ? defaultLCDIntensity : depthFocusedLCDIntensity;
        DOVirtual.Float(from, to, transitionDuration, val => lcdMaterial.SetFloat(IntensityPropertyID, val));
    }
}
