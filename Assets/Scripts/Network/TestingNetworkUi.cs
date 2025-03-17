using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetworkUi : NetworkBehaviour
{
    [SerializeField] private Button Host;
    [SerializeField] private Button Join;
    [SerializeField] private Button Spawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawn;
    private CustomCamera playerCam;
    void Awake()
    {
        Vector3 pos = spawn.position;
        Host.onClick.AddListener(() => { 
            print("HOST");
            NetworkManager.Singleton.StartHost();
            GameManager.Instance.SpawnPlayersServerRpc(pos.x, pos.y, pos.z);
            Hide();
        });
        Join.onClick.AddListener(() => { 
            print("CLIENT");
            NetworkManager.Singleton.StartClient();
           // GameManager.Instance.SpawnPlayersServerRpc(pos.x, pos.y, pos.z);
            Hide();
        });
        Spawn.onClick.AddListener(() => {
            print("Spawning");
            GameManager.Instance.SpawnPlayersServerRpc(pos.x, pos.y, pos.z);
            Hide();
        });
    }

    public void CycleUp()
    {
        print("Cycle up");
        if (playerCam == null)
        {
            playerCam = GameManager.Instance.Players[OwnerClientId].customCamera;
        }
        playerCam.CycleActiveUp();
    }

    public void CycleDown()
    {
        print("Cycle down");
        if (playerCam == null)
        {
            playerCam = GameManager.Instance.Players[OwnerClientId].customCamera;
        }
        playerCam.CycleActiveDown();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
