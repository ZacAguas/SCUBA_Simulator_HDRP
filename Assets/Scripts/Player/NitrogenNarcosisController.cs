using System;
using System.Collections;
using System.Collections.Generic;
using IE.RichFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using DG.Tweening;
using Random = UnityEngine.Random;

public class NitrogenNarcosisController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    private VolumeProfile profile;
    
    public int NarcosisLevel // 0 = not narced, 1-3 = narced, 4 = past MOD
    {
        get
        {
            return narcosisLevel;
        }
        set
        {
            if (value >= 0 && value <= 4)
                narcosisLevel = value;
        }
    }

    private int narcosisLevel; // backing field
    

    // Level 1
    private BloomStreak bloomStreak;
    private DirectionalBlur directionalBlur;
    private Sharpen sharpen;
    private FilmGrain filmGrain;
    // Level 2
    private DisplaceView displaceView;
    private Wobble wobble;

    [SerializeField] private float defaultBloomStreakIntensity;
    [SerializeField] private float defaultDirectionalBlurIntensity;
    [SerializeField] private float defaultSharpenIntensity;
    [SerializeField] private float defaultFilmGrainIntensity;
    [SerializeField] private Vector2 defaultDisplaceViewAmount;
    [SerializeField] private float defaultWobbleAmplitude;
    [SerializeField] private float defaultWobbleSpeed;

    private void Start()
    {
        NarcosisLevel = 0;

        var profile = volume.profile;
        if (!profile)
        {
            Debug.LogError("No profile on volume");
            return;
        }

        if (!profile.TryGet(out bloomStreak) || 
            !profile.TryGet(out directionalBlur) || 
            !profile.TryGet(out sharpen) ||
            !profile.TryGet(out filmGrain) ||
            !profile.TryGet(out displaceView) ||
            !profile.TryGet(out wobble))
        {
            Debug.LogError("Didn't get effect(s) from volume profile");
            return;
        }
        
        // initialising values
        
        // bloomStreak.intensity.value = defaultBloomStreakIntensity;
        // directionalBlur.intensity.value = defaultDirectionalBlurIntensity;
        // sharpen.intensity.value = defaultSharpenIntensity;
        // filmGrain.intensity.value = defaultFilmGrainIntensity;
        // displaceView.amount.value = defaultDisplaceViewAmount;
        // wobble.amplitude.value = defaultWobbleAmplitude;
        // wobble.speed.value = defaultWobbleSpeed;
        
        bloomStreak.intensity.value = 0;
        directionalBlur.intensity.value = 0;
        sharpen.intensity.value = 0;
        filmGrain.intensity.value = 0;
        displaceView.amount.value = Vector2.zero;
        wobble.amplitude.value = 0;
        wobble.speed.value = 0;



    }

    public void EnterNarcoticDepth()
    {
        StartCoroutine(NarcosisLoop());
    }

    public void ExitNarcoticDepth()
    {
        StopCoroutine(NarcosisLoop());
        
    }

    private IEnumerator NarcosisLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 5f)); // wait in between groups of pulses
            
            int iterations = Random.Range(1, 5); // number of repeats of this group of pulses
            float durationSecs = Random.Range(.2f, 2f); // duration of each pulse

            float longMultiplier = 3f; // multiplier for long effects
            float medMultiplier = 1f; // multiplier for med effects
            float shortMultiplier = .5f; // multiplier for short effects

            switch (NarcosisLevel)
            {
                case 0:
                    Debug.LogWarning("Shouldn't have narcotic level 0 inside this loop");
                    break;
                case 1:
                    sharpen.active = true;
                    filmGrain.active = true;
                    DOVirtual.Float(0, defaultSharpenIntensity, durationSecs * longMultiplier, val => sharpen.intensity.value = val)
                        .SetLoops(iterations, LoopType.Yoyo).SetEase(Ease.InBounce);
                    DOVirtual.Float(0, defaultFilmGrainIntensity, durationSecs * longMultiplier, val => filmGrain.intensity.value = val)
                        .SetLoops(iterations, LoopType.Yoyo).SetEase(Ease.InBounce);
                    
                    
                    
                    break;
                case 2:
                    bloomStreak.active = true;
                    displaceView.active = true;
                    break;
                case 3:
                    wobble.active = true;
                    directionalBlur.active = true;
                    break;
            }
            
        }
    }

}