using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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

    
    private PlayerMovement _player;
    public PlayerMovement Player => _player;

    private void Awake()
    {
        if (_instance == null){
            _instance = this;
        } else {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
        SaveSystem.Init();
    }

    public void StartRace()
    {
        SceneManager.LoadScene(1);
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
        
}
