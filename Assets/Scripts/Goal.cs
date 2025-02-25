using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private CourseManager courseManager;
    private void OnTriggerEnter(Collider collider)
    {  
        if (collider.gameObject.CompareTag("Player")) {
            TimeManager.Instance.StopwatchPause();
            courseManager.EndRace();
            //SaveSystem.SaveBySerialization();
            //SceneManager.LoadScene(0);
        }

    }
}
