using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {  
        if (collider.gameObject.CompareTag("Player")) {
            TimeManager.Instance.StopwatchPause();
            SaveObject save = new SaveObject(TimeManager.Instance.stopwatchTimer);
            GameManager.Instance.SaveTime(save);
        }

    }
}
