using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] protected TMP_InputField lobbyNameInputField;
    [SerializeField] protected Toggle privacyToggle;
    [SerializeField] protected GameObject createButton;
    [SerializeField] protected GameObject MenuFirst;
    [SerializeField] protected GameObject previousMenuFirst;
    [SerializeField] protected TextMeshProUGUI lobbyData;
    // [SerializeField] private GameObject passwordRequest;
    // [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] protected MenuManager menuManager;


    private void Awake()
    {
        ToggleCreateButton();
    }

    // public void TogglePasswordRequest()
    // {
    //     if (privacyToggle.isOn) {
    //         Show(passwordRequest);
    //     } else {
    //         Hide(passwordRequest);
    //     }
    // }

    
    public void ToggleCreateButton()
    {
        if (lobbyNameInputField.text != "") {
            Show(createButton);
        } else {
            Hide(createButton);
        }
    }

    public async void PlayAsHost()
    {
        menuManager.OpenLobbyMenu();
        GameManager.Instance.isOnline = true;
        Hide(this.gameObject);

        await GameLobby.Instance.CreateLobby(lobbyNameInputField.text, privacyToggle.isOn);
        Lobby lobby = GameLobby.Instance.GetLobby();
        lobbyData.text = "Lobby: " + lobby.Name + "\nCode: " + lobby.LobbyCode;
    }

    public void Show(GameObject gameObject)
    {
        //if (menuManager.director.state == PlayState.Playing) return;
        gameObject.SetActive(true);

        print(gameObject + " : " + this.gameObject);
        if (gameObject == this.gameObject) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(MenuFirst);
        }
    }

    public void Hide(GameObject gameObject)
    {
        if (gameObject == this.gameObject) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(previousMenuFirst);
        }
        
        gameObject.SetActive(false);
    }
}
