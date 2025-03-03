using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetworkUi : MonoBehaviour
{
    [SerializeField] private Button Host;
    [SerializeField] private Button Join;
    void Awake()
    {
        Host.onClick.AddListener(() => { 
            print("HOST");
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        Join.onClick.AddListener(() => { 
            print("CLIENT");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }


    
    private void Hide() {
        gameObject.SetActive(false);
    }
}
