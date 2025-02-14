using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private const float TRANSITION_SPEED = 0.5f;
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
        while (mainCamera.backgroundColor != target)
        {
            mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, target, TRANSITION_SPEED);
            print(mainCamera.backgroundColor);
            await Task.Delay(100);
        }
    }
}
