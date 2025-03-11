using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

public class JoinLobbyUI : CreateLobbyUI
{
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        Hide(this.gameObject);
        ToggleJoinButton();
        
    }

    public void ToggleJoinButton()
    {
        if (lobbyCodeInputField.text != "") {
            Show(joinButton.gameObject);
        } else {
            Hide(joinButton.gameObject);
        }
    }

    public async void JoinWithCode()
    {
        bool joined = await GameLobby.Instance.JoinWithCode(lobbyCodeInputField.text);
        if (!joined) {
            Debug.LogError("failed to join lobby");    
            return;
        }
        menuManager.OpenLobbyMenu();
        GameManager.Instance.isOnline = true;
        Hide(this.gameObject);

        Lobby lobby = GameLobby.Instance.GetLobby();
        lobbyData.text = "Lobby: " + lobby.Name + "\nCode: " + lobby.LobbyCode;
    }

    public async void PlayAsClient()
    {
        bool joined = await GameLobby.Instance.QuickJoin();
        if (!joined) {
            Debug.LogError("failed to join lobby");    
            return;
        }

        menuManager.OpenLobbyMenu();
        GameManager.Instance.isOnline = true;
        Hide(this.gameObject);

        Lobby lobby = GameLobby.Instance.GetLobby();
        lobbyData.text = "Lobby: " + lobby.Name + "\nCode: " + lobby.LobbyCode;
    }
}
