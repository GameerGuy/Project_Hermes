using UnityEngine;
using UnityEngine.InputSystem;

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


    private PlayerMovement player;

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }

    public void DisablePlayerInput()
    {
        if (player == null){
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        }
        player.DisableInput();
    }

    public void EnablePlayerInput()
    {
        if (player == null){
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        }
        player.EnableInput();
    }
        
}
