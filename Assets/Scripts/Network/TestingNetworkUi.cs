using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetworkUi : MonoBehaviour
{
    [SerializeField] private Button Host;
    [SerializeField] private Button Join;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;
    void Awake()
    {
        Host.onClick.AddListener(() => { 
            print("HOST");
            NetworkManager.Singleton.StartHost();
            PlayerMovement p = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponent<PlayerMovement>();
            p.GetComponent<NetworkObject>().SpawnAsPlayerObject(p.OwnerClientId);
            Hide();
        });
        Join.onClick.AddListener(() => { 
            print("CLIENT");
            NetworkManager.Singleton.StartClient();
            PlayerMovement p = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponent<PlayerMovement>();
            p.GetComponent<NetworkObject>().SpawnAsPlayerObject(p.OwnerClientId);
            Hide();
        });
    }


    
    private void Hide() {
        gameObject.SetActive(false);
    }
}
