using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CustomCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public void SetTarget(Transform player)
    {
        virtualCamera.Follow = virtualCamera.LookAt = player;
    }
}
