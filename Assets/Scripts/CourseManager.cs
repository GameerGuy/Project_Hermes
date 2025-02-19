using TMPro;
using UnityEngine;

public class CourseManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CustomCamera customCamera;
    [SerializeField] private TextMeshProUGUI countdownDisplay;
    [SerializeField] private TextMeshProUGUI stopwatchDisplay;
    private int countdownStart = 3;
    private bool countdownActive = false;

    private void Start()
    {
        GameManager.Instance.SetPlayer(Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponentInChildren<PlayerMovement>());
        GameManager.Instance.DisablePlayerInput();

        stopwatchDisplay.enabled = false;
        stopwatchDisplay.text = "Lap Time:\n" + TimeManager.Instance.stopwatchTimer.ToString("F3");

        countdownDisplay.enabled = false;
        countdownDisplay.text = "3";
        
        TimeManager.Instance.SetTimer( 0.5f, () =>{
            customCamera.CycleActiveDown();
            customCamera.SetTarget(GameManager.Instance.Player.transform);
        
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

    private void RaceCountdown(int time)
    {   
        countdownActive = true;
        countdownDisplay.fontSize = 200;
        countdownDisplay.color = new Color(countdownDisplay.color.r, countdownDisplay.color.g, countdownDisplay.color.b, 1);
        if (time > 0) {
            countdownDisplay.text = time.ToString();
            TimeManager.Instance.SetTimer(1, () => RaceCountdown(time-1));
            customCamera.CycleActiveDown();
            customCamera.SetTarget(GameManager.Instance.Player.transform);
        } else {
            customCamera.SetActiveCamera(0);
            countdownDisplay.text = "Go!";
            TimeManager.Instance.SetTimer(1, () => { countdownDisplay.enabled = false; });
            
            customCamera.SetTarget(GameManager.Instance.Player.transform);
            RaceStartTimer();
            countdownActive = false;
        }

    }

    private void RaceStartTimer()
    {
        stopwatchDisplay.enabled = true;
        TimeManager.Instance.StopwatchStart();
        GameManager.Instance.EnablePlayerInput();
    }

    private void CountdownChange()
    {
        countdownDisplay.fontSize -= 100 * Time.deltaTime;
        countdownDisplay.color += new Color(0, 0.66f * Time.deltaTime, 0.33f * Time.deltaTime, -Time.deltaTime);
    }

    
}
