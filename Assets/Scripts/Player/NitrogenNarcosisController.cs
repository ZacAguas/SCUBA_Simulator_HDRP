using System;
using System.Collections;
using System.Collections.Generic;
using IE.RichFX;
using UnityEngine;
using UnityEngine.Rendering;

public class NitrogenNarcosisController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    private VolumeProfile profile;

    private int narcosisLevel;

    // Level 1
    private BloomStreak bloomStreak;
    private DirectionalBlur directionalBlur;
    private Sharpen sharpen;
    // Level 2
    private DisplaceView displaceView;
    private Wobble wobble;


    private void Start()
    {
        narcosisLevel = 0;
        profile = volume.profile;

        // bloomStreak = profile.TryGet<BloomStreak>(
    }

    private void Update()
    {
    }
}