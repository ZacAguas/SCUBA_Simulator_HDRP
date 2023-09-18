using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LCDCamera : MonoBehaviour
{
    [SerializeField] private RenderTexture rt;
    private Camera lcdCam;

    private HDCameraUI hdCameraUI;


    private void Awake()
    {
        lcdCam = GetComponent<Camera>();
        hdCameraUI = GetComponent<HDCameraUI>();
    }


    private void Start()
    {
        lcdCam.targetTexture = rt;
        hdCameraUI.compositingMode = HDCameraUI.CompositingMode.Manual; // disables auto compositing ie. won't show up on main cam
    }
}
