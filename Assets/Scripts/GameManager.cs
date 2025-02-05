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
        _instance = this;
        DontDestroyOnLoad(this);
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
