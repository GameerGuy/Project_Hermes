using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MenuManager : NetworkBehaviour
{
    [SerializeField] private PlayableAsset intro;
    [SerializeField] private PlayableAsset openLevelSelect;
    [SerializeField] private PlayableAsset closeLevelSelect;
    [SerializeField] private PlayableAsset openLevelSelect_Lobby;
    [SerializeField] private PlayableAsset closeLevelSelect_Lobby;
    [SerializeField] private PlayableAsset openNetworkModeMenu;
    [SerializeField] private PlayableAsset closeNetworkModeMenu;
    [SerializeField] private PlayableAsset openLobby;
    [SerializeField] private PlayableAsset closelobby;
    [SerializeField] private PlayableAsset EnterLevel;
    [SerializeField] private TextMeshProUGUI WaitingForPlayersDisplay;
    private PlayableDirector director;
    private Camera mainCamera;
    private bool startingRace;

    private void Awake()
    {
        mainCamera = Camera.main;
        director = GetComponent<PlayableDirector>();
        director.Play(intro);
        startingRace = false;
    }

    public void PlayOffline()
    {
        if (director.state == PlayState.Playing) return;
        NetworkManager.Singleton.StartHost();
        GameManager.Instance.isOnline = false;
        OpenLevelSelectClientRpc();
    }

    public void ReadyUp()
    {
        GameManager.Instance.SetPlayerReadyServerRPC();
        if (GameManager.Instance.allPlayersReady) {
            OpenLevelSelectClientRpc();
            WaitingForPlayersDisplay.enabled = false;
        } else {
            WaitingForPlayersDisplay.enabled = true;
        }

    }

    public void OpenLobbyMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(openLobby);
    }

    public void CloseLobbyMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(closelobby);
        NetworkManager.Singleton.Shutdown();
        GameLobby.Instance.LeaveLobby();
    }

    public void OpenNetworkModeMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(openNetworkModeMenu);
    }

    public void CloseNetworkModeMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(closeNetworkModeMenu);
    }

    [ClientRpc]
    public void OpenLevelSelectClientRpc()
    {
        if (director.state == PlayState.Playing) return;
        if (GameManager.Instance.isOnline){
            director.Play(openLevelSelect_Lobby);
        } else {            
            director.Play(openLevelSelect);
        }
    }

    [ClientRpc]
    public void CloseLevelSelectClientRpc()
    {
        if (director.state == PlayState.Playing) return;
        if (GameManager.Instance.isOnline){
            director.Play(closeLevelSelect_Lobby);
        } else {            
            director.Play(closeLevelSelect);
        }
        NetworkManager.Singleton.Shutdown();
    }

    public void OpenOptionsMenu()
    {
        if (director.state == PlayState.Playing) return;
        print("options");
    }
    public void QuitGame()
    {
        GameManager.Instance.tokenSource.Cancel();
        Application.Quit();
    }

    public void StartRace(CourseData data)
    {
        if(startingRace) return;
        startingRace = true;

        GameManager.Instance.playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        data.UnpackColour(out float r, out float g, out float b, out float a);
        StartRaceClientRpc(data.courseName, r, g, b, a);
        GameManager.Instance.PlayerReady.Clear();
        GameLobby.Instance.DeleteLobby();
        GameLobby.Instance.Cleanup();
    }

    [ClientRpc]
    public void StartRaceClientRpc(string courseName, float r, float g, float b, float a)
    {
        Color backgroundColour = new Color(r, g, b, a);
        SceneTransition(courseName, backgroundColour);
    }

    public async void SceneTransition(string courseName, Color backgroundColour)
    {
        director.Play(EnterLevel);
        await GameManager.Instance.ChangeBackgroundColour(mainCamera, backgroundColour, GameManager.Instance.tokenSource.Token);
        await SceneManager.LoadSceneAsync(courseName);
    }

    
}
