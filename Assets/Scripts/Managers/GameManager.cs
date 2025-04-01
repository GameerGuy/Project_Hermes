using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private const float TRANSITION_TIME = 1f;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private GameObject playerPrefab;


    private Dictionary<ulong, PlayerMovement> _players;
    public Dictionary<ulong, PlayerMovement> Players => _players;


    private Dictionary<ulong, bool> _playerReady;
    public Dictionary<ulong, bool> PlayerReady => _playerReady;


    public CancellationTokenSource tokenSource = new();

    public const int MAX_PLAYER_COUNT = 4;
    public int playerCount = 1;
    public bool isOnline;
    public bool allPlayersReady {get; private set;}

    [SerializeField] public CourseData courseData;
    public List<Material> skyboxes;
    [SerializeField] private Material currentSkybox;



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
        _playerReady = new Dictionary<ulong, bool>();
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void PrintServerRpc(string text, ServerRpcParams serverRpcParams = default)
    {
        PrintClientRpc(serverRpcParams.Receive.SenderClientId.ToString() + ": " + text);

    }

    [ClientRpc]
    private void PrintClientRpc(string text)
    {
        print(text);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRPC(bool isReady, ServerRpcParams serverRpcParams = default)
    {
        _playerReady[serverRpcParams.Receive.SenderClientId] = isReady;

        bool _allPlayersReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!_playerReady.ContainsKey(clientId) || !_playerReady[clientId]) {
                _allPlayersReady = false;
                break;
            }
        }
        allPlayersReady = _allPlayersReady;
    }

    public void SetPlayerReadyFalse( ServerRpcParams serverRpcParams = default)
    {
        List<ulong> clients = (List<ulong>)NetworkManager.ConnectedClientsIds;
        foreach (ulong client in clients) {
            _playerReady[client] = false;
        }
        allPlayersReady = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayersServerRpc(float x, float y, float z, ServerRpcParams serverRpcParams = default)
    {
        Vector3 spawnPosition = new Vector3(x, y, z);
        
        Vector3 playerOffset = Vector3.zero;
        float spawnAngle = 0, currentSpawnAngle = 0;

        bool isMultiplayer = NetworkManager.ConnectedClients.Count > 1;
        if (isMultiplayer) {
            currentSpawnAngle = spawnAngle = 2 * Mathf.PI / NetworkManager.ConnectedClients.Count;
        }

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            
            if (isMultiplayer) {
                playerOffset = new Vector3(Mathf.Cos(currentSpawnAngle), 0, Mathf.Sin(currentSpawnAngle)) ;
                currentSpawnAngle += spawnAngle;
            }

            if (serverRpcParams.Receive.SenderClientId != clientId) continue;

            NetworkObject netObject = Instantiate(playerPrefab, spawnPosition + playerOffset, Quaternion.identity).GetComponent<NetworkObject>();
            netObject.SpawnAsPlayerObject(clientId);
            netObject.DestroyWithScene = true;



        }

    }

    public void RegisterPlayer(ulong id, PlayerMovement player)
    {
        _players.Add(id, player);
        //RegisterPlayerClientRpc(id);
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

    public void SetCourseData(CourseData data)
    {
        courseData = data;
        currentSkybox.Lerp(currentSkybox, skyboxes[data.skyboxIndex], 1);
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
            ResetCancellationToken(tokenSource);
            mainCamera.backgroundColor = startColour;
            throw;
        }
    }

    public async Task ChangeSkybox(CourseData data, CancellationToken token)
    {
        try {
            Material startMaterial = skyboxes[courseData.skyboxIndex];
            Material targetMaterial = skyboxes[data.skyboxIndex];

            GameObject Sun = GameObject.FindGameObjectWithTag("Sun");
            Quaternion startRotation = Sun.transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(data.sunRotation);

            float time = 0;
            while (time < TRANSITION_TIME) {
                
                currentSkybox.Lerp(startMaterial, targetMaterial, curve.Evaluate(time / (TRANSITION_TIME)));
                Sun.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curve.Evaluate(time/TRANSITION_TIME));
                time += Time.deltaTime;
                token.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        } catch (OperationCanceledException) {
            ResetCancellationToken(tokenSource);
            throw;
        }
    }

    public void ResetCancellationToken(CancellationTokenSource tokenSource)
    {
        if (tokenSource == null) return;

        tokenSource.Dispose();
        tokenSource = new CancellationTokenSource();

    }
        
}
