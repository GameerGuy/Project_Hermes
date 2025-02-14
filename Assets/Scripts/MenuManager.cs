using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private const float TRANSITION_TIME = 3f;
    [SerializeField] private AnimationCurve curve;
    private Camera mainCamera;


    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OpenOptionsMenu()
    {
        print("options");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public async void StartRace(CourseData data)
    {
        await ChangeBackgroundColour(data.backgroundColour);
        SceneManager.LoadScene(data.courseName);
    }

    private async Task ChangeBackgroundColour(Color target)
    {
        float time = 0;
        Color startColour = mainCamera.backgroundColor;
        while (time < TRANSITION_TIME) {
            mainCamera.backgroundColor = Color.Lerp(startColour, target, curve.Evaluate(time/TRANSITION_TIME));
            time += Time.deltaTime;
            print(time);
            await UniTask.Yield();
        }
    }
}
