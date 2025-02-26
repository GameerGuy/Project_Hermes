using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const float TRANSITION_TIME = 1f;
    [SerializeField] private AnimationCurve curve;
    private PlayerMovement _player;
    public PlayerMovement Player => _player;
    public CancellationTokenSource tokenSource = new();

    private static GameManager _instance;
    public static GameManager Instance
    {
        get 
        {
            if (_instance == null){
                Debug.LogError("Singleton null");
            }
            return _instance;
        }

    }

    private void Awake()
    {
        if (_instance == null){
            _instance = this;
            DontDestroyOnLoad(this);
            SaveSystem.Init();
            Application.targetFrameRate = -1;
        } else {
            Destroy(gameObject);
        }
    }

    public void SetPlayer(PlayerMovement p)
    {
        _player = p;
    }

    public void DisablePlayerInput()
    {
        if (_player == null) return;
        _player.DisableInput();
    }

    public void EnablePlayerInput()
    {
        if (_player == null) return;
        _player.EnableInput();
    }

    public async Task ChangeBackgroundColour(Camera mainCamera , Color target, CancellationToken token)
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
