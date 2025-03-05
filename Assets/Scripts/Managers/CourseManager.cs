using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CourseManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private TextMeshProUGUI countdownDisplay;
    [SerializeField] private TextMeshProUGUI stopwatchDisplay;
    [SerializeField] private GameObject levelClearMenu;

    private List<CustomCamera> playerCams = new List<CustomCamera>();
    private int countdownStart = 3;
    private bool countdownActive = false;

    private void Start()
    {
        SpawnPlayers(GameManager.Instance.isOnline);

        TimeManager.Instance.StopwatchClear();

        stopwatchDisplay.enabled = false;
        stopwatchDisplay.text = "Lap Time:\n" + TimeManager.Instance.stopwatchTimer.ToString("F3");

        countdownDisplay.enabled = false;
        countdownDisplay.text = "3";

        levelClearMenu.SetActive(false);
        
        TimeManager.Instance.SetTimer( 0.5f, () => {
            playerCams[0].CycleActiveDown();
        
            TimeManager.Instance.SetTimer( 1f, () => {
                countdownDisplay.enabled = true;
                RaceCountdown(countdownStart);
            });
        });
    }

    private void Update()
    {
        stopwatchDisplay.text = "Lap Time:\n" + TimeManager.Instance.stopwatchTimer.ToString("F3");
        if (!countdownActive) return;

        CountdownChange();
    }

    private void SpawnPlayers(bool isOnline)
    {
        if (!isOnline) {
            // NetworkManager.Singleton.StartHost();
            PlayerMovement p = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponent<PlayerMovement>();
            p.GetComponent<NetworkObject>().SpawnAsPlayerObject(p.OwnerClientId);
            p.DisableInput();
            playerCams.Add(p.customCamera);
        }
    }

    private void RaceCountdown(int time)
    {   
        countdownActive = true;
        countdownDisplay.fontSize = 200;
        countdownDisplay.color = new Color(countdownDisplay.color.r, countdownDisplay.color.g, countdownDisplay.color.b, 1);
        if (time > 0) {
            countdownDisplay.text = time.ToString();
            playerCams[0].CycleActiveDown();
            TimeManager.Instance.SetTimer(1, () => RaceCountdown(time-1));
        } else {
            playerCams[0].SetActiveCamera(1);
            countdownDisplay.text = "Go!";
            TimeManager.Instance.SetTimer(1, () => { countdownDisplay.enabled = false; });
            
            RaceStartTimer();
            countdownActive = false;
        }

    }

    private void RaceStartTimer()
    {
        stopwatchDisplay.enabled = true;
        TimeManager.Instance.StopwatchStart();
        GameManager.Instance.EnableAllPlayersInput();
    }

    private void CountdownChange()
    {
        countdownDisplay.fontSize -= 100 * Time.deltaTime;
        countdownDisplay.color += new Color(0, 0.66f * Time.deltaTime, 0.33f * Time.deltaTime, -Time.deltaTime);
    }

    public void EndRace()
    {
        GameManager.Instance.DisableAllPlayersInput();
        playerCams[0].SetActiveCamera(0);

        countdownDisplay.text = "Finish!"; 
        countdownDisplay.enabled = true;

        TimeManager.Instance.SetTimer( 1f, () => {
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
        playerCams[0].ActivateEnd();
        await GameManager.Instance.ChangeBackgroundColour(playerCams[0].GetCamera(), data.backgroundColour, GameManager.Instance.tokenSource.Token);
        SceneManager.LoadScene("Start Screen");
        GameManager.Instance.Players.Clear();
        GameLobby.Instance.Disconnect();
        //GameLobby.Instance.Cleanup();
    }


}
