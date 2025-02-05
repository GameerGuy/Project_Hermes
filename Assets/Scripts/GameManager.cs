using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

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

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }
        
}
