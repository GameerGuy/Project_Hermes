using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private const float TRANSITION_TIME = 1f;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private PlayableAsset intro;
    [SerializeField] private PlayableAsset openLevelSelect;
    [SerializeField] private PlayableAsset closeLevelSelect;
    [SerializeField] private PlayableAsset EnterLevel;
    private CancellationTokenSource tokenSource = new();
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
        tokenSource.Cancel();
        Application.Quit();
    }

    public async void StartRace(CourseData data)
    {
        if(startingRace) return;
        director.Play(EnterLevel);
        startingRace = true;
        await ChangeBackgroundColour(data.backgroundColour, tokenSource.Token);
        SceneManager.LoadScene(data.courseName);
    }

    private async Task ChangeBackgroundColour(Color target, CancellationToken token)
    {
        Color startColour = mainCamera.backgroundColor;
        try {
            float time = 0;
            while (time < TRANSITION_TIME) {
                mainCamera.backgroundColor = Color.Lerp(startColour, target, curve.Evaluate(time/TRANSITION_TIME));
                time += Time.deltaTime;
                token.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        } catch (OperationCanceledException){
            mainCamera.backgroundColor = startColour;
            throw;
        }
    }
}
