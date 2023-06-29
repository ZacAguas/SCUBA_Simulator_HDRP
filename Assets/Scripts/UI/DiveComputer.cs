using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiveComputer : MonoBehaviour
{
    
    [SerializeField] private Shader lcdShader;
    private Material lcdMaterial;

    private void Awake()
    {
        lcdMaterial = new Material(lcdShader);
    }
}
