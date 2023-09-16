using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private float leftRightInput;
    private float forwardBackInput;
    private float upDownInput;
    private float inflateDeflateInput;
    private float mouseX;
    private float mouseY;
    private bool toggleMode; // for toggling camera mode

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        leftRightInput = Input.GetAxis("Horizontal");
        forwardBackInput = Input.GetAxis("Vertical");
        upDownInput = Input.GetAxis("Ascend");
        inflateDeflateInput = Input.GetAxisRaw("BCD"); // raw input to prevent smoothing
        
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        toggleMode = Input.GetButtonDown("Toggle Mode");
    }

    public Vector3 GetSwimInput()
    {
        return new Vector3(leftRightInput, upDownInput, forwardBackInput).normalized;
    }

    public Vector2 GetMouseInput()
    {
        return new Vector2(mouseX, mouseY);
    }

    public float GetBCDInput()
    {
        return inflateDeflateInput;
    }

    public bool GetToggleMode()
    {
        return toggleMode;
    }

}
