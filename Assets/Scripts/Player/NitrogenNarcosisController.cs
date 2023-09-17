using System;
using System.Collections;
using System.Collections.Generic;
using IE.RichFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using DG.Tweening;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NitrogenNarcosisController : MonoBehaviour
{
    private InputManager inputManager; // for camera FX toggle
    private DepthManager depthManager; // need depth value for changing FX eg. exposure

    [SerializeField] private Volume globalVolume; // for exposure etc.
    [SerializeField] private WaterSurface waterSurface; // for absorption distance multiplier modified by depth
    [SerializeField] private Volume narcosisVolume;
    
    public int NarcosisLevel // 0 = not narced, 1-3 = narced, 4 = past MOD
    {
        get => narcosisLevel;
        set
        {
            if (value >= 0 && value <= 4)
                narcosisLevel = value;
        }
    }
    private int narcosisLevel; // backing field

    // Visibility
    [SerializeField] private float minExposureCompensation;
    [SerializeField] private float maxExposureCompensation;
    [SerializeField] private float maxAbsorptionDistanceMultiplier;
    [SerializeField] private Color deepScatteringColor;
    // for checking in inspector
    [SerializeField] private Color currentScatteringColor;

    private Exposure exposure;
    private float defaultExposureCompensation = 0;
    [SerializeField] private float defaultAbsorptionDistanceMultiplier;
    [SerializeField] private Color defaultScatteringColor;
    
    // Level 1
    private BloomStreak bloomStreak;
    private DirectionalBlur directionalBlur;
    private Sharpen sharpen;
    private FilmGrain filmGrain;
    // Level 2
    private DisplaceView displaceView;
    private Wobble wobble;
    // Camera FX
    private Pixelate pixelate;
    private RGBSplit rgbSplit;
    private ChromaLines chromaLines;
    private ScreenFuzz screenFuzz;
    private Nightvision nightVision;
    

    [SerializeField] private bool enableCameraFX; // whether or not to use camera fx

    private Tween currentTween; // the current tween we are waiting for to finish

    [SerializeField] private float defaultBloomStreakIntensity;
    [SerializeField] private float defaultDirectionalBlurIntensity;
    [SerializeField] private float defaultSharpenIntensity;
    [SerializeField] private float defaultFilmGrainIntensity;
    [SerializeField] private Vector2 defaultDisplaceViewAmount;
    [SerializeField] private float defaultWobbleAmplitude;
    [SerializeField] private float defaultWobbleSpeed;
    
    // camera fx
    [SerializeField] private int defaultPixelateIntensity = 25;
    [SerializeField] private float defaultRGBSplitIntensity = .2f;
    [SerializeField] private float defaultChromaLinesIntensity = .33f;
    [SerializeField] private float defaultScreenFuzzIntensity = .265f;
    [SerializeField] private float defaultNightVisionDarkness = .95f;

    private void Start()
    {
        inputManager = GetComponent<InputManager>();
        depthManager = GetComponent<DepthManager>();
        
        NarcosisLevel = 0;

        InitNarcosisVolume();
        InitGlobalVolume();
        waterSurface.scatteringColor = defaultScatteringColor; // reset scattering
    }

    private void InitNarcosisVolume()
    {
        var profile = narcosisVolume.profile;
        if (!profile)
        {
            Debug.LogError("No profile on narcosis volume");
            return;
        }

        if (!profile.TryGet(out bloomStreak) ||
            !profile.TryGet(out directionalBlur) ||
            !profile.TryGet(out sharpen) ||
            !profile.TryGet(out filmGrain) ||
            !profile.TryGet(out displaceView) ||
            !profile.TryGet(out wobble) ||
            !profile.TryGet(out pixelate) ||
            !profile.TryGet(out rgbSplit) ||
            !profile.TryGet(out chromaLines) ||
            !profile.TryGet(out screenFuzz) ||
            !profile.TryGet(out nightVision))
        {
            Debug.LogError("Didn't get effect(s) from narcosis volume profile");
            return;
        }

        // initialising values

        bloomStreak.intensity.value = 0;
        directionalBlur.intensity.value = 0;
        sharpen.intensity.value = 0;
        filmGrain.intensity.value = 0;
        displaceView.amount.value = Vector2.zero;
        wobble.amplitude.value = 0;
        wobble.speed.value = 0;
        // camera FX
        pixelate.intensity.value = 0;
        rgbSplit.intensity.value = 0;
        chromaLines.intensity.value = 0;
        screenFuzz.intensity.value = 0;
        nightVision.darkness.value = 0;
    }

    private void InitGlobalVolume()
    {
        var profile = globalVolume.profile;
        if (!profile)
        {
            Debug.LogError("No profile on global volume");
            return;
        }

        if (!profile.TryGet(out exposure))
        {
            Debug.LogError("Didn't get effect(s) from global volume profile");
            return;
        }

        // initialising values

        exposure.compensation.value = 0;
    }

    private void Update()
    {
        CheckToggleMode();
        AdjustVisibility();
    }

    private void CheckToggleMode()
    {
        // only continue on the frame we toggle
        if (!Input.GetKeyDown(KeyCode.C)) return;

        // effects are currently on, turn them off
        if (enableCameraFX)
        {
            pixelate.active = false;
            rgbSplit.active = false;
            chromaLines.active = false;
            screenFuzz.active = false;
            nightVision.active = false;

            pixelate.intensity.value = 0;
            rgbSplit.intensity.value = 0;
            chromaLines.intensity.value = 0;
            screenFuzz.intensity.value = 0;
            nightVision.darkness.value = 0;

            enableCameraFX = false; // effects are now off
        }
        else
        {
            pixelate.active = true;
            rgbSplit.active = true;
            chromaLines.active = true;
            screenFuzz.active = true;
            nightVision.active = true;

            pixelate.intensity.value = defaultPixelateIntensity;
            rgbSplit.intensity.value = defaultRGBSplitIntensity;
            chromaLines.intensity.value = defaultChromaLinesIntensity;
            screenFuzz.intensity.value = defaultScreenFuzzIntensity;
            nightVision.darkness.value = defaultNightVisionDarkness;

            enableCameraFX = true;
        }
    }

    private void AdjustVisibility()
    {
        // interpolate scattering colour darker, absorption distance higher, (exposure compensation?) with depth
        float depth = depthManager.Depth;
        float maxDepth = 300;
        float normalisedDepth = depth / maxDepth;
        
        // waterSurface.scatteringColor = Color.Lerp(defaultScatteringColor, deepScatteringColor, normalisedDepth);
        currentScatteringColor = Color.Lerp(defaultScatteringColor, deepScatteringColor, normalisedDepth);
        waterSurface.scatteringColor = currentScatteringColor;
        waterSurface.absorptionDistanceMultiplier = Mathf.Lerp(defaultAbsorptionDistanceMultiplier,maxAbsorptionDistanceMultiplier, normalisedDepth);;


    }

    public void EnterNarcoticDepth()
    {
        if (!currentTween.IsActive())
            StartCoroutine(NarcosisLoop());
    }

    private IEnumerator NarcosisLoop()
    {
        while (true)
        {
            int iterations; // number of repetitions for each group of pulses (depends on narcosis level)
            
            
            // durations of each pulse
            float shortDuration = Random.Range(.2f, .5f); 
            float medDuration = Random.Range(2f, 4f);
            float longDuration = Random.Range(10f, 15f);
            
            switch (NarcosisLevel)
            {
                case 0: // outside of narcosis depth
                    if (currentTween.IsActive())
                        yield return currentTween.WaitForCompletion(); // wait for tween to complete
                    yield break; // exit coroutine
                case 1:
                    Debug.Log("Level 1");
                    
                    yield return new WaitForSeconds(Random.Range(10f, 25f)); // wait in between groups of pulses
                    iterations = Random.Range(1, 5);
                    
                    
                    sharpen.active = true;
                    filmGrain.active = true;
                    
                    // Sharpen
                    currentTween = DOVirtual.Float(0, defaultSharpenIntensity, medDuration, val => sharpen.intensity.value = val)
                        .SetLoops(iterations * 2, LoopType.Yoyo) // iterations * 2 because yoyo loop type counts iteration as each direction
                        .SetEase(Ease.InBounce);
                    // Film grain
                    currentTween = DOVirtual.Float(0, defaultFilmGrainIntensity, medDuration, val => filmGrain.intensity.value = val)
                        .SetLoops(iterations * 2, LoopType.Yoyo)
                        .SetEase(Ease.InBounce);
                    
                    yield return currentTween.WaitForCompletion();
                    yield break;
                case 2:
                    Debug.Log("Level 2");
                    
                    yield return new WaitForSeconds(Random.Range(2f, 15f));
                    iterations = Random.Range(3, 9);
                    
                    displaceView.active = true;
                    currentTween = DOVirtual.Vector2(Vector2.zero, defaultDisplaceViewAmount, shortDuration, val => displaceView.amount.value = val)
                        .SetLoops(iterations * 2, LoopType.Yoyo) // iterations * 2 because yoyo loop type counts iteration as each direction
                        .SetEase(Ease.InExpo);
                    yield return currentTween.WaitForCompletion();
                    yield break;
                case 3:
                    Debug.Log("Level 3");
                    
                    yield return new WaitForSeconds(Random.Range(1f, 5f));
                    iterations = Random.Range(8,15);
                    
                    wobble.active = true;
                    directionalBlur.active = true;
                    
                    currentTween = DOVirtual.Float(0, defaultWobbleAmplitude, longDuration, val => wobble.amplitude.value = val)
                        .SetLoops(1, LoopType.Yoyo)
                        .SetEase(Ease.OutElastic);

                    yield return currentTween.WaitForCompletion();
                    yield break;
                default:
                    Debug.LogWarning("Invalid narcotic level");
                    yield break;
            }
            
        }
    }
}