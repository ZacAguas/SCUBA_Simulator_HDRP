using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VesselDoorTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    
    private bool playerInsideTrigger;
    private float currentTime = 0;
    [SerializeField] private float timeUntilRefusal;

    private IEnumerator Tick()
    {
        while (currentTime < timeUntilRefusal)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
        
        // timer has finished
        gameManager.Die(CauseOfDeath.Refusal, 3, false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // start timer
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Tick());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // reset timer
        if (!other.CompareTag("Player")) return;
        
        playerInsideTrigger = false;
        StopCoroutine(Tick());
        currentTime = 0;
    }
}
