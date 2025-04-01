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
    [SerializeField] private CourseData courseData;
    [SerializeField] private PlayableAsset intro;
    [SerializeField] private PlayableAsset openOptionsMenu;
    [SerializeField] private PlayableAsset closeOptionsMenu;
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
        GameManager.Instance.SetCourseData(courseData);
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
        director.Play(openOptionsMenu);
    }

    public void CloseOptionsMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(closeOptionsMenu);
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

        //data.UnpackColour(out float r, out float g, out float b, out float a);
        StartRaceListClientRpc(data);
        GameManager.Instance.SetPlayerReadyFalse();
        GameLobby.Instance.DeleteLobby();
        GameLobby.Instance.Cleanup();
    }

    [ClientRpc]
    public void StartRaceListClientRpc(CourseData data)
    {
        SceneTransition(data);

        GameManager.Instance.SetCourseData(data);
    }

    public async void SceneTransition(CourseData data)
    {
        director.Play(EnterLevel);
        await GameManager.Instance.ChangeSkybox(data, GameManager.Instance.tokenSource.Token);
        
        if (!IsServer) return;
        NetworkManager.SceneManager.LoadScene(data.courseName, LoadSceneMode.Single);
    }

    
}
