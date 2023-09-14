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
        get => narcosisLevel;
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

    private Tween currentTween; // the current tween we are waiting for to finish

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
        if (!currentTween.IsActive())
            StartCoroutine(NarcosisLoop());
    }

    private IEnumerator NarcosisLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 5f)); // wait in between groups of pulses
            
            int iterations = Random.Range(1, 5); // number of repeats of this group of pulses
            
            // durations of each pulse
            float shortDuration = Random.Range(.25f, .75f); 
            float medDuration = Random.Range(1f, 2f);
            float longDuration = Random.Range(2f, 4f);
            
            switch (NarcosisLevel)
            {
                case 0: // outside of narcosis depth
                    if (currentTween.IsActive())
                        yield return currentTween.WaitForCompletion(); // wait for tween to complete
                    yield break; // exit coroutine
                case 1:
                    Debug.Log("Level 1");
                    sharpen.active = true;
                    filmGrain.active = true;
                    currentTween = DOVirtual.Float(0, defaultSharpenIntensity, longDuration, val => sharpen.intensity.value = val)
                        .SetLoops(iterations * 2, LoopType.Yoyo) // iterations * 2 because yoyo loop type counts iteration as each direction
                        .SetEase(Ease.InBounce);
                    currentTween = DOVirtual.Float(0, defaultFilmGrainIntensity, longDuration, val => filmGrain.intensity.value = val)
                        .SetLoops(iterations * 2, LoopType.Yoyo)
                        .SetEase(Ease.InBounce);
                    
                    // directionalBlur.active = true;
                    // currentTween = DOVirtual.Float(0, defaultDirectionalBlurIntensity, longDuration, val => directionalBlur.intensity.value = val)
                    // .SetLoops(iterations * 2, LoopType.Yoyo)
                    // .SetEase(Ease.Linear); 
                    
                    yield return currentTween.WaitForCompletion();
                    break;
                case 2:
                    bloomStreak.active = true;
                    displaceView.active = true;
                    Debug.Log("Level 2");
                    break;
                case 3:
                    wobble.active = true;
                    directionalBlur.active = true;
                    Debug.Log("Level 3");

                    break;
            }
            
        }
    }
}