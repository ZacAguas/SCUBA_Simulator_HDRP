using System;
using System.Collections;
using System.Collections.Generic;
using IE.RichFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class NitrogenNarcosisController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    private VolumeProfile profile;

    private int narcosisLevel;

    // Level 1
    private BloomStreak bloomStreak;
    private DirectionalBlur directionalBlur;
    private Sharpen sharpen;
    private FilmGrain filmGrain;
    // Level 2
    private DisplaceView displaceView;
    private Wobble wobble;


    private void Start()
    {
        narcosisLevel = 0;

        var profile = volume.profile;
        if (!profile)
        {
            Debug.LogError("No profile on volume");
            return;
        }

        if (!profile.TryGet<BloomStreak>(out bloomStreak))
        {
            Debug.LogError("No bloom streak found");
            return;
        }

        bloomStreak.active = true;

    }

    private void Update()
    {
    }
}