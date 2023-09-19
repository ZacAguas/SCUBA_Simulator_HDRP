using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GearController : MonoBehaviour
{
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

    private void Update()
    {
        FocusEquipment();
    }

    private void FocusEquipment()
    {
        // Left click & not currently focused on other side
        if (Input.GetMouseButtonDown(0) && !isFocusedRight)
        {
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
}
