using System.Threading;
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

    public void OpenNetworkModeMenu()
    {
        director.Play(openNetworkModeMenu);
    }

    public void CloseNetworkModeMenu()
    {
        director.Play(closeNetworkModeMenu);
    }

    public void OpenLevelSelect()
    {
        director.Play(openLevelSelect);
    }

    public void CloseLevelSelect()
    {
        director.Play(closeLevelSelect);
    }

    public void OpenOptionsMenu()
    {
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
