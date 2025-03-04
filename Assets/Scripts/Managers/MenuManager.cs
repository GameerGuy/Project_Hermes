using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private PlayableAsset intro;
    [SerializeField] private PlayableAsset openLevelSelect;
    [SerializeField] private PlayableAsset closeLevelSelect;
    [SerializeField] private PlayableAsset openNetworkModeMenu;
    [SerializeField] private PlayableAsset closeNetworkModeMenu;
    [SerializeField] private PlayableAsset EnterLevel;
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

    public async void PlayAsHost()
    {
        await GameLobby.Instance.CreateLobby("Lobby Name", false);
        GameManager.Instance.isOnline = true;
        OpenLevelSelect();
    }

    public async void PlayAsClient()
    {
        await GameLobby.Instance.QuickJoin();
        GameManager.Instance.isOnline = true;
        OpenLevelSelect();
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
        director.Play(openLevelSelect);
    }

    public void CloseLevelSelect()
    {
        if (director.state == PlayState.Playing) return;
        director.Play(closeLevelSelect);
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
