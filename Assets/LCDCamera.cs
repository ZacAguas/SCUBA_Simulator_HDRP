using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LCDCamera : MonoBehaviour
{
    [SerializeField] private RenderTexture renderTexture;
    private Camera renderCamera;

    private void Start()
    {
        renderCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();
        renderCamera.targetTexture = null;
    }
}
