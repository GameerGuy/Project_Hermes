using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ControlsMenu : MonoBehaviour
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private Toggle sprintToggle;


    public void Show(GameObject gameObject)
    {
        if (menuManager.director.state == PlayState.Playing) return;
        gameObject.SetActive(true);
    }

    public void Hide(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public void SetToggleSprint()
    {
        
        PlayerPrefs.SetInt(InputManager.TOGGLE_SPRINT_KEY, sprintToggle.isOn ? 1 : 0);
    }
}
