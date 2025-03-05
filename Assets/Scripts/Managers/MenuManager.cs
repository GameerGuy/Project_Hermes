using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
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
        OpenLevelSelect();
    }

    public void ReadyUp()
    {
        GameManager.Instance.SetPlayerReadyServerRPC();
        if (GameManager.Instance.allPlayersReady) {
            OpenLevelSelect();
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
        GameLobby.Instance.Disconnect();
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

    public void OpenLevelSelect()
    {
        if (director.state == PlayState.Playing) return;
        if (GameManager.Instance.isOnline){
            director.Play(openLevelSelect_Lobby);
        } else {            
            director.Play(openLevelSelect);
        }
    }

    public void CloseLevelSelect()
    {
        if (director.state == PlayState.Playing) return;
        if (GameManager.Instance.isOnline){
            director.Play(closeLevelSelect_Lobby);
        } else {            
            director.Play(closeLevelSelect);
        }
        GameLobby.Instance.Disconnect();
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

    public async void StartRace(CourseData data)
    {
        if(startingRace) return;
        director.Play(EnterLevel);
        startingRace = true;
        await GameManager.Instance.ChangeBackgroundColour(mainCamera, data.backgroundColour, GameManager.Instance.tokenSource.Token);
        SceneManager.LoadScene(data.courseName);
    }

    
}
