using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float timeInSeconds;
    void Start()
    {
        timeInSeconds = 0;
    }

    void Update()
    {
        timeInSeconds += Time.deltaTime;
    }

    public string TimeInMMSS()
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}
