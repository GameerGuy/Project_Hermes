using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CourseManager : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TextMeshProUGUI countdownDisplay;
    [SerializeField] private TextMeshProUGUI stopwatchDisplay;
    [SerializeField] private GameObject levelClearMenu;
    [SerializeField] private TextMeshProUGUI waitingForPlayersDisplay;

    [SerializeField] private PlayableAsset returnToMenu;
    public TimeManager timeManager { get; private set; }
    private CancellationTokenSource tokenSource = new();
    private PlayableDirector director;
    private CustomCamera playerCam;
    private int countdownStart = 3;
    private bool countdownActive = false;
    private bool dnfTimerActive = false;
    private bool raceEnded = false;
    private bool returningToMenu = false;


    void Awake()
    {
        timeManager = GetComponent<TimeManager>();
        director = GetComponent<PlayableDirector>();
        Vector3 pos =  spawnPoint.position;
        GameManager.Instance.SpawnPlayersServerRpc(pos.x, pos.y, pos.z);
        GetComponent<NetworkObject>().DestroyWithScene = true;
    }

    private void Start()
    {
        GameManager.Instance.DisableAllPlayersInput();
        timeManager.StopwatchClear();

        stopwatchDisplay.enabled = false;
        stopwatchDisplay.text = "Lap Time:\n" + timeManager.stopwatchTimer.ToString("F3");

        countdownDisplay.enabled = false;
        countdownDisplay.text = "3";

        levelClearMenu.SetActive(false);
        
        timeManager.SetTimer( 1f, () => {
            playerCam = GameManager.Instance.Players[NetworkManager.LocalClientId].customCamera;
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
        if (id != NetworkManager.LocalClientId) {
            timeManager.SetTimer(5f, () => {
                DNFTimer(5, tokenSource.Token);
            });
            return;

        } 

        timeManager.StopwatchPause();
        raceEnded = true;

        GameManager.Instance.DisablePlayerInput(id);
        playerCam.SetActiveCamera(0);

        countdownDisplay.text = "Finish!"; 
        countdownDisplay.enabled = true;

        waitingForPlayersDisplay.enabled = false;

        if (dnfTimerActive) {
            tokenSource.Cancel();
        }

        timeManager.SetTimer(1f, () => {
            if (countdownDisplay != null) {
                countdownDisplay.enabled = false;
            }
            if (levelClearMenu != null) {
                levelClearMenu.SetActive(true);
            }
        });
    }



    private async void DNFTimer(float duration, CancellationToken token)
    {
        try {

            if (dnfTimerActive || raceEnded) return;
            dnfTimerActive = true;

            countdownDisplay.color = Color.red;
            countdownDisplay.text = duration.ToString("F0");
            countdownDisplay.enabled = true;
        
            while (duration > 0) {
                token.ThrowIfCancellationRequested();

                countdownDisplay.text = duration.ToString("F0");
                await UniTask.Yield();
                duration -= Time.deltaTime;
            }

            raceEnded = true;

            GameManager.Instance.DisableAllPlayersInput();
            playerCam.SetActiveCamera(0);

            countdownDisplay.text = "DNF!";
            waitingForPlayersDisplay.enabled = false;

            await Task.Delay(1000, token);

            if (countdownDisplay != null) {
                countdownDisplay.enabled = false;
            }
            if (levelClearMenu != null) {
                levelClearMenu.SetActive(true);
            }


        } catch (OperationCanceledException e)
        {
            print(e);
            countdownDisplay.enabled = false;
            GameManager.Instance.ResetCancellationToken(tokenSource);
        }
    }

    public void ReplayLevel()
    {
        GameManager.Instance.SetPlayerReadyServerRPC(true);
        if (GameManager.Instance.allPlayersReady) {
            ReplayLevelServerRpc();
        }
        else {
            waitingForPlayersDisplay.enabled = true;
        }
        
    }

    [ServerRpc]
    public void ReplayLevelServerRpc()
    {
        ReplayLevelClientRpc();
        GameManager.Instance.SetPlayerReadyFalse();
        NetworkManager.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    [ClientRpc]
    public void ReplayLevelClientRpc()
    {
        GameManager.Instance.Players.Clear();
    }

    public async void ReturnToMenu(CourseData data)
    {
        try {
            if (!returningToMenu && NetworkManager.LocalClientId == NetworkManager.ServerClientId) {
                //data.UnpackColour(out float r, out float g, out float b, out float a);
                ReturnToMenuClientRpc(data);
                return;
            }

            playerCam.ActivateEnd();
            director.Play(returnToMenu);

            //await GameManager.Instance.ChangeBackgroundColour(playerCam.GetCamera(), data.backgroundColour, tokenSource.Token);
            await GameManager.Instance.ChangeSkybox(data, GameManager.Instance.tokenSource.Token);

            GameManager.Instance.Players.Clear();
            GameManager.Instance.PlayerReady.Clear();

            NetworkManager.Singleton.Shutdown();

            GameLobby.Instance.Cleanup();

            await SceneManager.LoadSceneAsync(data.courseName, LoadSceneMode.Single);
        } catch (OperationCanceledException e)
        {
            print(e);
            GameManager.Instance.ResetCancellationToken(tokenSource);
        }
    }

    [ClientRpc]
    private void ReturnToMenuClientRpc(CourseData data)
    {
        returningToMenu = true;
        ReturnToMenu(data);
    }
}
