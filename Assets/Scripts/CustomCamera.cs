using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomCamera : NetworkBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private CinemachineVirtualCamera[] virtualCameras;
    public CinemachineVirtualCamera activeCamera { get; private set;}
    public int activeIndex { get; private set;}

    private void Awake()
    {
        ActivateEnd();
        Canvas[] canvas = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvas) {
            c.worldCamera = cam;
        }
    }

    private void Start()
    {
        SetCameraColour(GameManager.Instance.courseData.backgroundColour);
    }

    public Camera GetCamera()
    {
        return cam;
    }

    //[ServerRpc(RequireOwnership = false)]
    public void SetActiveCamera(int index) 
    {
        if (index == activeIndex) return;

        int length = BoundsCheck(index);
        if (length < 0) return;

        for (int i = 0; i < length; i++)
        {
            if (i != index)
            {
                virtualCameras[i].Priority = 0;
                continue;
            }

            virtualCameras[i].Priority = 1;
            activeCamera = virtualCameras[i];
            activeIndex = i;


            CinemachinePOV followCam = virtualCameras[i].GetCinemachineComponent<CinemachinePOV>();
            if (followCam != null) {
                SetSensitivity(followCam);
            }
        }
        //SetActiveCameraClientRpc(index, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> {serverRpcParams.Receive.SenderClientId} } });        
    }

    [ClientRpc]
    public void SetActiveCameraClientRpc(int index, ClientRpcParams clientRpcParams) {
        if (index == activeIndex) return;

        int length = BoundsCheck(index);
        if (length < 0) return;

        for (int i = 0; i < length; i++) {
            if (i != index) {
                virtualCameras[i].Priority = 0;
                continue;
            }

            virtualCameras[i].Priority = 1;
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
        if (index >= length || index < 0){
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


    public void SetCameraColour(Color backgroundColour)
    {
        cam.backgroundColor = backgroundColour;
    }

    public void SetSensitivity(CinemachinePOV followCam)
    {
        float sens = PlayerPrefs.GetFloat(InputManager.LOOK_SENSITIVITY_KEY);
        if (sens == 0) {
            sens = 0.2f;
            PlayerPrefs.SetFloat(InputManager.LOOK_SENSITIVITY_KEY, sens);
        }
        followCam.m_HorizontalAxis.m_MaxSpeed = sens;
        followCam.m_VerticalAxis.m_MaxSpeed = sens;
    }



    [ServerRpc(RequireOwnership = false)]
    public void SetCameraColourServerRpc()
    {
        print("server set colour");
        SetCameraColourClientRpc(GameManager.Instance.courseData);
    }

    [ClientRpc]
    public void SetCameraColourClientRpc(CourseData data)
    {
        print("client set colour");
        cam.backgroundColor = data.backgroundColour;
    }

    public float GetRotation()
    {
        return activeCamera.transform.rotation.eulerAngles.y;
    }
}
