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

    private void Start()
    {
        GameManager.Instance.SetPlayer(Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponentInChildren<PlayerMovement>());
        GameManager.Instance.DisablePlayerInput();

        customCamera.SetTarget(GameManager.Instance.Player.transform);

        stopwatchDisplay.enabled = false;
        stopwatchDisplay.text = "Lap Time:\n" + TimeManager.Instance.stopwatchTimer.ToString("F3");
        RaceCountdown(countdownStart);
    }

    private void Update()
    {
        stopwatchDisplay.text = "Lap Time:\n" + TimeManager.Instance.stopwatchTimer.ToString("F3");
        if (stopwatchDisplay.enabled) return;

        CountdownChange();
    }

    private void RaceCountdown(int time)
    {
        countdownDisplay.fontSize = 200;
        countdownDisplay.color = new Color(countdownDisplay.color.r, countdownDisplay.color.g, countdownDisplay.color.b, 1);
        if (time > 0) {
            countdownDisplay.text = time.ToString();
            TimeManager.Instance.SetTimer(1, () => RaceCountdown(time-1));
        } else {
            countdownDisplay.text = "Go!";
            TimeManager.Instance.SetTimer(1, () => { countdownDisplay.enabled = false; });
            RaceStartTimer();
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
        countdownDisplay.color = new Color(countdownDisplay.color.r, countdownDisplay.color.g + 0.66f * Time.deltaTime, countdownDisplay.color.b + 0.33f * Time.deltaTime, countdownDisplay.color.a - Time.deltaTime);
    }

    
}
