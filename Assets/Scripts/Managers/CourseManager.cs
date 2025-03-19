using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CourseManager : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TextMeshProUGUI countdownDisplay;
    [SerializeField] private TextMeshProUGUI stopwatchDisplay;
    [SerializeField] private GameObject levelClearMenu;
    [SerializeField] private TextMeshProUGUI waitingFoPlayersDisplay;
    [SerializeField] private PlayerMovement player;
    public TimeManager timeManager;
    private CustomCamera playerCam;
    private int countdownStart = 3;
    private bool countdownActive = false;


    void Awake()
    {
        timeManager = GetComponent<TimeManager>();
        Vector3 pos =  spawnPoint.position;
        GameManager.Instance.SpawnPlayersServerRpc(pos.x, pos.y, pos.z);
        GameManager.Instance.DisableAllPlayersInput();
    }

    private void Start()
    {
        print(OwnerClientId);
        timeManager.StopwatchClear();

        stopwatchDisplay.enabled = false;
        stopwatchDisplay.text = "Lap Time:\n" + timeManager.stopwatchTimer.ToString("F3");

        countdownDisplay.enabled = false;
        countdownDisplay.text = "3";

        levelClearMenu.SetActive(false);
        
        timeManager.SetTimer( 1f, () => {
            print(NetworkManager.LocalClientId);
            player = GameManager.Instance.Players[NetworkManager.LocalClientId];
            playerCam = player.customCamera;
            playerCam.CycleActiveDown();

            timeManager.SetTimer( 1f, () => {
                countdownDisplay.enabled = true;
                RaceCountdown(countdownStart);
            });
        });
    }

    private void Update()
    {
        stopwatchDisplay.text = "Lap Time:\n" + timeManager.stopwatchTimer.ToString("F3");
        if (!countdownActive) return;

        CountdownChange();
    }
    

    private void RaceCountdown(int time)
    {   
        countdownActive = true;
        countdownDisplay.fontSize = 200;
        countdownDisplay.color = new Color(countdownDisplay.color.r, countdownDisplay.color.g, countdownDisplay.color.b, 1);
        if (time > 0) {
            countdownDisplay.text = time.ToString();
            playerCam.CycleActiveDown();
            timeManager.SetTimer(1, () => RaceCountdown(time-1));
        } else {
            countdownDisplay.text = "Go!";
            playerCam.SetActiveCamera(1);
            timeManager.SetTimer(1, () => { countdownDisplay.enabled = false; });
            
            RaceStartTimer();
            countdownActive = false;
        }
    }

    private void RaceStartTimer()
    {
        stopwatchDisplay.enabled = true;
        timeManager.StopwatchStart();
        GameManager.Instance.EnableAllPlayersInput();
    }

    private void CountdownChange()
    {
        countdownDisplay.fontSize -= 100 * Time.deltaTime;
        countdownDisplay.color += new Color(0, 0.66f * Time.deltaTime, 0.33f * Time.deltaTime, -Time.deltaTime);
    }

    public void EndRace(ulong id)
    {
        if (id != NetworkManager.LocalClientId) return;
        
        GameManager.Instance.DisableAllPlayersInput();
        playerCam.SetActiveCamera(0);

        countdownDisplay.text = "Finish!"; 
        countdownDisplay.enabled = true;

        timeManager.SetTimer( 1f, () => {
            if (countdownDisplay != null) {
                countdownDisplay.enabled = false;
            }
            if (levelClearMenu != null) {                
                levelClearMenu.SetActive(true);
            }
        });
    }

    public void ReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.Instance.Players.Clear();
    }

    public async void ReturnToMenu(CourseData data)
    {
        playerCam.ActivateEnd();
        await GameManager.Instance.ChangeBackgroundColour(playerCam.GetCamera(), data.backgroundColour, GameManager.Instance.tokenSource.Token);
        await SceneManager.LoadSceneAsync(data.courseName);
        GameManager.Instance.Players.Clear();
        GameManager.Instance.PlayerReady.Clear();
        NetworkManager.Singleton.Shutdown();
        GameLobby.Instance.Cleanup();
    }
}
