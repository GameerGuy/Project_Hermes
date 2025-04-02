using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MenuManager : NetworkBehaviour
{
    [SerializeField] private CourseData courseData;
    
    [Header("Buttons")]
    [SerializeField] private GameObject mainMenuFirst;
    [SerializeField] private GameObject optionsMenuFirst;
    [SerializeField] private GameObject networkModeMenuFirst;
    [SerializeField] private GameObject lobbyMenuFirst;
    [SerializeField] private GameObject levelSelectMenuFirst;

    [Header("Animations")]
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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(lobbyMenuFirst);
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

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(networkModeMenuFirst);
        }

    }

    [ClientRpc]
    public void CloseLobbyMenuClientRpc()
    {
        director.Play(closelobby);
        NetworkManager.Singleton.Shutdown();

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(networkModeMenuFirst);
    }

    public void OpenNetworkModeMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(openNetworkModeMenu);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(networkModeMenuFirst);
    }

    public void CloseNetworkModeMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(closeNetworkModeMenu);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(levelSelectMenuFirst);
    }

    [ClientRpc]
    public void CloseLevelSelectClientRpc()
    {
        if (director.state == PlayState.Playing) return;

        if (GameManager.Instance.isOnline){
            director.Play(closeLevelSelect_Lobby);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(lobbyMenuFirst);
        } else {            
            director.Play(closeLevelSelect);
            NetworkManager.Singleton.Shutdown();

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(networkModeMenuFirst);
        }

        if (GameLobby.Instance.IsLobbyHost()) {
            GameManager.Instance.SetPlayerReadyServerRPC(false);
        }
    }

    public void OpenOptionsMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(openOptionsMenu);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsMenuFirst);
    }

    public void CloseOptionsMenu()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(closeOptionsMenu);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);
    }

    public void QuitGame()
    {
        GameManager.Instance.tokenSource.Cancel();
        Application.Quit();
    }

    public void StartRace(CourseData data)
    {
        if(startingRace || director.state == PlayState.Playing) return;
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
