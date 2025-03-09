using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private CinemachineVirtualCamera[] virtualCameras;
    private CinemachineVirtualCamera activeCamera;
    private int activeIndex;

    void Awake()
    {
        ActivateEnd();
        Canvas[] canvas = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvas) {
            c.worldCamera = cam;
        }
    }

    public Camera GetCamera()
    {
        return cam;
    }

    public void SetActiveCamera(int index) {

        if (index == activeIndex) return;

        int length = BoundsCheck(index);
        if (length < 0) return;

        for (int i = 0; i < length; i++) {
            if (i != index) {
                virtualCameras[i].gameObject.SetActive(false);
                continue;
            }

            virtualCameras[i].gameObject.SetActive(true);
            activeCamera = virtualCameras[i];
            activeIndex = i;
        }        
    }

    public void CycleActiveUp() {
        SetActiveCamera(activeIndex + 1);
    }

    public void CycleActiveDown() {
        SetActiveCamera(activeIndex - 1);
    }

    public void ActivateEnd()
    {
        SetActiveCamera(virtualCameras.Length -1);
    }

    public void Deactivate()
    {
        cam.enabled = false;
    }

    private int BoundsCheck(int index){
        int length = virtualCameras.Length;
        if (index > length || index < 0){
            Debug.LogError("camera index out of bounds");
            return -1;
        }
        return length;

    }


    public void SetTargetForAll(Transform player)
    {
        for (int i = 0; i < virtualCameras.Length; i++) {
            virtualCameras[i].Follow = virtualCameras[i].LookAt = player;
        }
    }

    public void SetTarget(Transform player)
    {
        activeCamera.Follow = activeCamera.LookAt = player;
    }
}
