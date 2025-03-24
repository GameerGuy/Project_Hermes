using System;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private CourseManager courseManager;

    void Awake()
    {
        courseManager = FindObjectOfType<CourseManager>();
    }
    private void OnTriggerEnter(Collider collider)
    {  
        if (collider.gameObject.CompareTag("Player")) {
            courseManager.EndRace(collider.GetComponent<PlayerMovement>().OwnerClientId);
        }

    }
}
