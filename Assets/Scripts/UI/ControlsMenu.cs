using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ControlsMenu : MonoBehaviour
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private GameObject MenuFirst;
    [SerializeField] private GameObject previousMenuFirst;
    [SerializeField] private Toggle sprintToggle;
    [SerializeField] private Slider lookSensSlider;


    private void Awake()
    {
        sprintToggle.isOn = PlayerPrefs.GetInt(InputManager.TOGGLE_SPRINT_KEY, 1) != 0;
    }

    public void Show(GameObject gameObject)
    {
        if (menuManager.director.state == PlayState.Playing) return;
        gameObject.SetActive(true);

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

    public void SetToggleSprint()
    {
        PlayerPrefs.SetInt(InputManager.TOGGLE_SPRINT_KEY, sprintToggle.isOn ? 1 : 0);
    }

    public void SetLookSensitivity()
    {
        PlayerPrefs.SetFloat(InputManager.LOOK_SENSITIVITY_KEY, lookSensSlider.value);
        print(PlayerPrefs.GetFloat(InputManager.LOOK_SENSITIVITY_KEY));
    }
}
