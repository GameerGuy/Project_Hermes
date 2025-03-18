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
    public PlayableDirector director {get; private set;}
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
        GameManager.Instance.StartHost();
        GameManager.Instance.isOnline = false;
        OpenLevelSelectClientRpc();
    }

    public void ReadyUp()
    {
        GameManager.Instance.SetPlayerReadyServerRPC(true);
        if (GameManager.Instance.allPlayersReady) {
            OpenLevelSelectServerRpc();
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

        WaitingForPlayersDisplay.enabled = false;
        if (GameLobby.Instance.IsLobbyHost()) {

            CloseLobbyMenuClientRpc();
            GameLobby.Instance.DeleteLobby();
            GameManager.Instance.PlayerReady.Clear();

        } else {

            director.Play(closelobby);
            NetworkManager.Singleton.Shutdown();
            GameLobby.Instance.LeaveLobby();
            GameManager.Instance.PlayerReady.Remove(OwnerClientId);
        }

    }

    [ClientRpc]
    public void CloseLobbyMenuClientRpc()
    {
        director.Play(closelobby);
        NetworkManager.Singleton.Shutdown();
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

    [ServerRpc(RequireOwnership = false)]
    private void OpenLevelSelectServerRpc()
    {
        OpenLevelSelectClientRpc();
    }

    [ClientRpc]
    private void OpenLevelSelectClientRpc()
    {
        if (director.state == PlayState.Playing) return;
        WaitingForPlayersDisplay.enabled = false;
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
            NetworkManager.Singleton.Shutdown();
        }
        if (GameLobby.Instance.IsLobbyHost()) {
            GameManager.Instance.SetPlayerReadyServerRPC(false);
        }
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

        GameManager.Instance.SetCourseData(data);
        data.UnpackColour(out float r, out float g, out float b, out float a);
        StartRaceClientRpc(data.courseName, r, g, b, a);
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
