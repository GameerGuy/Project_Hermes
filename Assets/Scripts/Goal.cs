using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {  
        if (collider.gameObject.CompareTag("Player")) {
            TimeManager.Instance.StopwatchPause();
            SaveSystem.SaveBySerialization();
            SceneManager.LoadScene(0);
        }

    }
}
