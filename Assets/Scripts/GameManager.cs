using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Timer timer;
    [SerializeField] private DepthManager depthManager;

    [SerializeField] private float depthRequiredToAscendPerSecond;
    
    private bool isDying; // can be reverted

    private CauseOfDeath causeOfDeath;
    private float depth;
    private float waitSeconds;
    private bool checkAfterWait;

    [SerializeField] private CanvasGroup bgCanvasGroup;
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private CanvasGroup descriptionCanvasGroup;
    [SerializeField] private float bgFadeDuration;
    [SerializeField] private float textFadeDuration;
    
    [SerializeField] private GameObject deathUI;
    [SerializeField] private RawImage bgImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private void Awake()
    {
        timer = GetComponent<Timer>();
        
    }

    private void Start()
    {
        deathUI.SetActive(false);
        bgCanvasGroup.alpha = 0;
        titleCanvasGroup.alpha = 0;
        descriptionCanvasGroup.alpha = 0;
    }

    public void Die(CauseOfDeath causeOfDeath, float waitSeconds, bool checkAfterWait)
    {
        if (isDying) return; // only do anything if player isn't already dying
        
        // assign values
        this.causeOfDeath = causeOfDeath;
        this.waitSeconds = waitSeconds;
        this.checkAfterWait = checkAfterWait;
        depth = depthManager.Depth;
        StartCoroutine(Die());
    }

    private string GenerateDescription(string minutes, string seconds)
    {
        string intDepth = Mathf.RoundToInt(this.depth).ToString();
        string description = "";

        switch (causeOfDeath)
        {
            case CauseOfDeath.Depth:
                description =
                    $"You convulsed and died due to oxygen toxicity at {intDepth}m after {minutes} minutes and {seconds} seconds. " +
                    "Your body will not be recovered.";
                break;
            case CauseOfDeath.AscentRate:
                description =
                    $"You surfaced too quickly after {minutes} minutes and {seconds} seconds and suffered severe decompression sickness " +
                    "where you ultimately lost consciousness and drowned. You were not revived.";
                break;
            case CauseOfDeath.Refusal:
                description = $"You refused to participate and as a result were purged from the recovery team. Your family still wait for you.";
                break;
            case CauseOfDeath.OutOfAir:
                description =
                    $"You were unable to surface in time and had your last breath at {intDepth}m after {minutes} minutes " +
                    $"and {seconds} seconds. Your body was later recovered. It is unclear whether you were ever returned to your family.";
                break;
        }

        return description;
    }

    private IEnumerator Die()
    {
        isDying = true;
        yield return new WaitForSeconds(waitSeconds);

        if (checkAfterWait) // player might not be dead
        {
            switch (causeOfDeath)
            {
                case CauseOfDeath.Depth when depthManager.Depth < depth - depthRequiredToAscendPerSecond * waitSeconds:
                    Debug.Log("Player has decreased depth by enough; don't kill");
                    isDying = false;
                    yield break;
                case CauseOfDeath.Depth:
                    Debug.Log("Player too deep, kill player");
                    break;
                case CauseOfDeath.AscentRate when depthManager.CurrentAscentRate <= depthManager.MaxAscentRate:
                    Debug.Log("Player has slowed ascent enough; don't kill");
                    isDying = false;
                    yield break;
                case CauseOfDeath.AscentRate:
                    Debug.Log("Player has not slowed ascent enough, kill player");
                    break;
                default:
                    Debug.LogWarning("Unexpected check for cause of death");
                    isDying = false;
                    yield break;
            }
        }
        // player is dead
        string minutes = timer.Minutes.ToString();
        string seconds = timer.Seconds.ToString();
        string description = GenerateDescription(minutes, seconds);
            
        descriptionText.text = description; 
        
        deathUI.SetActive(true);
        Tween tween = bgCanvasGroup.DOFade(1, bgFadeDuration).SetEase(Ease.InBounce); // fade UI in
        yield return tween.WaitForCompletion();
        tween = titleCanvasGroup.DOFade(1, textFadeDuration).SetDelay(1f);
        yield return tween.WaitForCompletion();
        tween = descriptionCanvasGroup.DOFade(1, textFadeDuration).SetDelay(.5f);
        yield return tween.WaitForCompletion();

        // end game logic
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Main_Scene");
    }
}

public enum CauseOfDeath
{
    OutOfAir,
    AscentRate,
    Depth,
    Refusal
}
