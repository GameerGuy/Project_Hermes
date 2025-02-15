using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {  
        if (collider.gameObject.CompareTag("Player")) {
            TimeManager.Instance.StopwatchPause();
            SaveObject save = new SaveObject {
                courseName = SceneManager.GetActiveScene().name,
                lapTime = TimeManager.Instance.stopwatchTimer,
                date = DateTime.Today.ToShortDateString(),
                time = DateTime.Now.ToShortTimeString()
            };
            SaveSystem.Save(save);
            SceneManager.LoadScene(0);
        }

    }
}
