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
        await GameLobby.Instance.JoinWithCode(lobbyCodeInputField.text);

        menuManager.OpenLobbyMenu();
        GameManager.Instance.isOnline = true;
        Hide(this.gameObject);

        Lobby lobby = GameLobby.Instance.GetLobby();
        lobbyData.text = "Lobby: " + lobby.Name + "\nCode: " + lobby.LobbyCode;
    }

    public async void PlayAsClient()
    {
        await GameLobby.Instance.QuickJoin();

        menuManager.OpenLobbyMenu();
        GameManager.Instance.isOnline = true;
        Hide(this.gameObject);

        Lobby lobby = GameLobby.Instance.GetLobby();
        print(lobby.ToString());
        lobbyData.text = "Lobby: " + lobby.Name + "\nCode: " + lobby.LobbyCode;
    }
}
