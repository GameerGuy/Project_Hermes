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

    private float _stopwatchTimer;
    public float stopwatchTimer => _stopwatchTimer;
    private bool stopwatchActive = false;

    private Action timerCallback;
    private float timer;

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        if (stopwatchActive){
            _stopwatchTimer += Time.deltaTime;
        }

        if (timer > 0){
            timer -= Time.deltaTime;

            if (timer <= 0){
                timerCallback();
            }
        }
    }

    public void StopwatchStart()
    {
        stopwatchActive = true;
    }

    public float StopwatchPuase()
    {
        stopwatchActive = false;
        return _stopwatchTimer;
    }

    public void StopwatchClear()
    {
        _stopwatchTimer = 0;
        StopwatchPuase();
    }

    public void SetTimer(float time, Action action){
        this.timer = time;
        this.timerCallback = action;
    }

}
