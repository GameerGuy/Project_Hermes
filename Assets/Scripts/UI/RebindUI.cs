using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindUI : MonoBehaviour
{
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private bool excludeMouse = true;
    
    [Range(0, 10)][SerializeField]
    private int selectedBinding;

    [Header("Binding Info - DON'T TOUCH")]
    [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;
    [SerializeField] private InputBinding inputBinding;
    
    private int bindingIndex;
    private string actionName;

    [Header("UI Fields")]
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private TMP_Text rebindText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Button resetButton;


    private void OnEnable()
    {
        rebindButton.onClick.AddListener(() => DoRebind());
        resetButton.onClick.AddListener(() => ResetBinding());

        if (inputActionReference != null) {
            InputManager.LoadBindingOverride(actionName);
            GetBindingInfo();
            UpdateUI();
        }

        InputManager.rebindComplete += UpdateUI;
        InputManager.rebindCancelled += UpdateUI;
    }

    private void OnDisable()
    {
        InputManager.rebindComplete -= UpdateUI;
        InputManager.rebindCancelled -= UpdateUI;
    }

    private void OnValidate()
    {
        if (inputActionReference == null) return;

        GetBindingInfo();
        UpdateUI();
    }

    private void GetBindingInfo()
    {
        if (inputActionReference.action != null) {
            actionName = inputActionReference.action.name;
        }

        if (inputActionReference.action.bindings.Count > selectedBinding) {
            inputBinding = inputActionReference.action.bindings[selectedBinding];
            bindingIndex = selectedBinding;
        }
    }

    private void UpdateUI()
    {
        if (actionText != null){
            actionText.text = actionName;
        }

        if (rebindText != null) {
            if (Application.isPlaying) {
                rebindText.text = InputManager.GetBindingName(actionName, bindingIndex);
            } else {
                rebindText.text = inputActionReference.action.GetBindingDisplayString(bindingIndex);
            }
        }
    }

    private void DoRebind()
    {
        InputManager.StartRebind(actionName, bindingIndex, rebindText, excludeMouse);
    }

    private void ResetBinding()
    {
        InputManager.ResetBinding(actionName, bindingIndex);
        UpdateUI();
    }

}
