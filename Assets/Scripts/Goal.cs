using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {  
        if (collider.gameObject.CompareTag("Player")) {
            TimeManager.Instance.StopwatchPause();
            SaveObject save = new SaveObject{
                lapTime = TimeManager.Instance.stopwatchTimer, 
                date = DateTime.Today.ToShortDateString()
            };
            SaveSystem.Save(save);
        }

    }
}
