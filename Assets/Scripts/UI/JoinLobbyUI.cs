using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    public void JoinWithCode()
    {
        GameLobby.Instance.JoinWithCode(lobbyCodeInputField.text);
    }
}
