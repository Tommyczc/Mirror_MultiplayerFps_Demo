using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputModule : GameModule
{
    private GameInput _input;
    private Dictionary<string, Action<InputAction.CallbackContext>> inputmap;
    public override void OnRegister()
    {
        _input = new GameInput();
        inputmap = new Dictionary<string, Action<InputAction.CallbackContext>>();
        _input.Enable();
    }

    public GameInput Input => _input;

    private void FixedUpdate()
    {
        if(inputmap.Count==0)return;
        foreach (string inputName in inputmap.Keys)
        {
            if (_input.FindAction(inputName) != null)
            {
                inputmap[inputName].Invoke(default);
            }
        }
    }

    public void BindPerformedAction(string inputName, Action<InputAction.CallbackContext> action)
    {
        _input.FindAction(inputName).performed += action;
    }

    public void UnbindPerformedAction(string inputName, Action<InputAction.CallbackContext> action)
    {
        _input.FindAction(inputName).performed -= action;
    }

    public void SetActionMapActive(string inputMapName, bool state)
    {
        InputActionMap targetMap = _input.asset.FindActionMap(inputMapName);
        if (state) targetMap.Enable();
        else targetMap.Disable();
    }

    public void SetActionActive(string inputActionName, bool state)
    {
        InputAction targetAction = _input.FindAction(inputActionName);
        if (state) targetAction.Enable();
        else targetAction.Disable();
    }

    public override void OnUnregister()
    {
        //
    }
}
