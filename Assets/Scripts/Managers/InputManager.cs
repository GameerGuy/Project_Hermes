using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public const string TOGGLE_SPRINT_KEY = "ToggleStrintTrue";
    public const string LOOK_SENSITIVITY_KEY = "LookSens";
    
    public static PlayerInput inputActions;

    public static event Action rebindComplete;
    public static event Action rebindCancelled;
    public static event Action<InputAction, int> rebindStarted;

    private void Awake()
    {
        if (inputActions == null) {
            inputActions = new PlayerInput();
        }
    }

    public static void EnableInput()
    {
        if (inputActions != null){
            inputActions.Player.Enable();
        }
    }

    public static void DisableInput()
    {
        if (inputActions != null){
            inputActions.Player.Disable();
        }
    }


    public static void StartRebind(string actionName, int bindingIndex, TMP_Text statusText, bool excludeMouse)
    {
        InputAction action = inputActions.asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= bindingIndex) {
            Debug.LogError("Couldn't find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite) {
            int firstIndex = bindingIndex + 1;
            if (firstIndex < action.bindings.Count && action.bindings[firstIndex].isPartOfComposite) {
                DoRebind(action, firstIndex, statusText, true, excludeMouse);
            } 
        } else {
            DoRebind(action, bindingIndex, statusText, false, excludeMouse);
        }
    }
    

    private static void DoRebind(InputAction actionToRebind, int bindingIndex, TMP_Text statusText, bool allCompositeParts, bool excludeMouse)
    {
        if (actionToRebind == null || bindingIndex < 0) return;

        statusText.text = "><";

        actionToRebind.Disable();

        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

        rebind.OnComplete(operation => {
            actionToRebind.Enable();
            operation.Dispose();

            if (allCompositeParts) {
                int nextBindingindex = bindingIndex + 1;
                if (nextBindingindex < actionToRebind.bindings.Count && actionToRebind.bindings[nextBindingindex].isPartOfComposite) {
                    DoRebind(actionToRebind, nextBindingindex, statusText, allCompositeParts, excludeMouse);
                }
            }
            
            SaveBindingOverride(actionToRebind);
            rebindComplete?.Invoke();
        });

        rebind.OnCancel(operation => {
            actionToRebind.Enable();
            operation.Dispose();

            rebindCancelled?.Invoke();
        });

        rebind.WithCancelingThrough("<Keyboard>/escape");

        if (excludeMouse) {
            rebind.WithControlsExcluding("Mouse");
        }

        rebindStarted?.Invoke(actionToRebind, bindingIndex);
        rebind.Start();

    }

    public static string GetBindingName(string actionName, int bindingIndex)
    {
        if (inputActions == null){
            inputActions = new PlayerInput();
        }

        InputAction action = inputActions.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    public static void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++) {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    public static void LoadBindingOverride(string actionName)
    {
        if (inputActions == null){
            inputActions = new PlayerInput();
        }
        
        InputAction action = inputActions.asset.FindAction(actionName);

        for (int i = 0; i < action.bindings.Count; i++) {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i))) {
                action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }
    }

    public static void ResetBinding(string actionName, int bindingIndex)
    {
        if (inputActions == null){
            inputActions = new PlayerInput();
        }
        
        InputAction action = inputActions.asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= bindingIndex) {
            Debug.LogError("Couldn't find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite) {
            for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                action.RemoveBindingOverride(i);
            }
        } else {
            action.RemoveBindingOverride(bindingIndex);
        }

        SaveBindingOverride(action);

    }
}
