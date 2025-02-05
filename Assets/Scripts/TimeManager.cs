using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager _instance;
    public static TimeManager Instance
    {
        get 
        {
            if (_instance == null){
                Debug.LogError("Singleton null");
            }
            return _instance;
        }

    }

    private float stopwatchTimer;
    private bool stopwatchActive = false;

    private void Awake()
    {
        _instance = this;


    }

    private void Update()
    {
        if (stopwatchActive){
            stopwatchTimer += Time.deltaTime;
        }
        print(stopwatchTimer);

    }

    public void StopwatchStart()
    {
        stopwatchActive = true;
    }

    public void StopwatchPuase()
    {
        stopwatchActive = false;
    }

    public void StopwatchClear()
    {
        stopwatchTimer = 0;
        StopwatchPuase();
    }

}
