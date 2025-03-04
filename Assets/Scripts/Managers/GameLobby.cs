using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    private void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        InitializeUnityAuthentication();
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
            await LobbyService.Instance.QuickJoinLobbyAsync();
            NetworkManager.Singleton.StartClient();
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public void Cleanup()
    {
        if (NetworkManager.Singleton != null) {
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }

}
