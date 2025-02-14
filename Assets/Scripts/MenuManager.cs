using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
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

    public async void StartRace()
    {
        await ChangeBackgroundColour(new Color(236, 184, 127));
        SceneManager.LoadScene(1);
    }

    private async Task ChangeBackgroundColour(Color target)
    {
        target /= 255;
        print(target);
        while (mainCamera.backgroundColor != target)
        {
            mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, target, 0.5f);
            print(mainCamera.backgroundColor);
            await Task.Delay(100);
        }
        print("Change complete");
    }
}
