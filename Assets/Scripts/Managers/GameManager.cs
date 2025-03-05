using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private const float TRANSITION_TIME = 1f;

    [SerializeField] private AnimationCurve curve;

    private Dictionary<ulong, PlayerMovement> _players;
    public Dictionary<ulong, PlayerMovement> Players => _players;
    private Dictionary<ulong, bool> playerReady;

    public CancellationTokenSource tokenSource = new();

    public const int MAX_PLAYER_COUNT = 4;
    public int playerCount = 1;
    public bool isOnline;
    public bool allPlayersReady {get; private set;}


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
        _players = new Dictionary<ulong, PlayerMovement>();
        playerReady = new Dictionary<ulong, bool>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRPC(ServerRpcParams serverRpcParams = default)
    {
        playerReady[serverRpcParams.Receive.SenderClientId] = true;

        bool _allPlayersReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerReady.ContainsKey(clientId) || !playerReady[clientId]){
                _allPlayersReady = false;
                break;
            }
        }
        allPlayersReady = _allPlayersReady;
    }

    public void RegisterPlayer(ulong id, PlayerMovement player)
    {
        _players.Add(id, player);
        print(id);
    }

    public void EnableAllPlayersInput()
    {
        foreach (PlayerMovement pm in _players.Values)
        {
            pm.EnableInput();
        }
    }

    public void DisableAllPlayersInput()
    {
        foreach (PlayerMovement pm in _players.Values)
        {
            pm.DisableInput();
        }
    }

    public void EnablePlayerInput(ulong id)
    {
        if (_players == null) return;
        _players[id].EnableInput();
    }

    public void DisablePlayerInput(ulong id)
    {
        if (_players == null) return;
        _players[id].DisableInput();
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
            ResetCancellationToken();
            mainCamera.backgroundColor = startColour;
            throw;
        }
    }

    public void ResetCancellationToken()
    {
        if (tokenSource == null) return;

        tokenSource.Dispose();
        tokenSource = new CancellationTokenSource();

    }
        
}
