using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public void OpenOptionsMenu()
    {
        print("options");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartRace()
    {
        SceneManager.LoadScene(1);
    }
}
