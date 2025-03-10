using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public static GameLobby Instance {  get; private set; }

    private Lobby joinedLobby;
    private float heartBeatTimer;

    private void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        InitializeUnityAuthentication();
    }


    private void Update()
    {
        HandleHeartBeat();
    }

    private void HandleHeartBeat()
    {
        if (!IsLobbyHost()) return;
        heartBeatTimer -= Time.deltaTime;

        if (heartBeatTimer <= 0){
            float heartBeatTimerMax = 15f;
            heartBeatTimer = heartBeatTimerMax;

            LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
        }

    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0, 1000).ToString());
            
            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async Task CreateLobby(string lobbyName, bool isPrivate)
    {
        try {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameManager.MAX_PLAYER_COUNT, new CreateLobbyOptions{
                IsPrivate = isPrivate,
            });
            NetworkManager.Singleton.StartHost();

        } catch(LobbyServiceException e) {
            Debug.Log(e);
        } 
        
    }

    public async Task QuickJoin()
    {
        try {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            NetworkManager.Singleton.StartClient();
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async Task JoinWithCode(string lobbyCode)
    {
        try {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            NetworkManager.Singleton.StartClient();
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try{
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            print("Lobbies: " + queryResponse.Results.Count);
            foreach(Lobby lobby in queryResponse.Results){
                print(lobby.Name + " " + lobby.LobbyCode);
            }
        } catch(LobbyServiceException e){
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    public void Cleanup()
    {
        var networkManagers = FindObjectsByType<NetworkManager>(FindObjectsSortMode.InstanceID);
        for(int i = networkManagers.Length - 1; i > 0 ; i--)
        {
            Destroy(networkManagers[i].gameObject);
        }
    }

}
