using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float timeInSeconds;
    public int Minutes => Mathf.FloorToInt(timeInSeconds / 60);
    public int Seconds => Mathf.FloorToInt(timeInSeconds % 60);
    
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
        return $"{Minutes:00}:{Seconds:00}";
    }

}
