using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{


    private float _stopwatchTimer;
    public float stopwatchTimer => _stopwatchTimer;
    private bool stopwatchActive = false;

    private Action timerCallback;
    private float timer;

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

    public void StopwatchPause()
    {
        stopwatchActive = false;
    }

    public void StopwatchClear()
    {
        _stopwatchTimer = 0;
        StopwatchPause();
    }

    public void SetTimer(float time, Action action){
        this.timer = time;
        this.timerCallback = action;
    }

}
