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

    [SerializeField] private RawImage img;

    private void Awake()
    {
        lcdCam = GetComponent<Camera>();
    }


    private void Start()
    {
        lcdCam.targetTexture = rt;
    }

    private void Update()
    {
        img.color = Random.ColorHSV();
    }
}
