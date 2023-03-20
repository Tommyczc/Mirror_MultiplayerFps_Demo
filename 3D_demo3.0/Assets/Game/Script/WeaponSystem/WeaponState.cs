using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WeaponState : MonoBehaviour
{
    private WeaponIdentification id;

    private bool updateInput=false;

    public UnityAction fire;

    private InputModule _inputModule;

    private void Awake()
    {
        _inputModule = App.Modules.Get<InputModule>();
    }

    // Start is called before the first frame update
    void Start()
    {
        id = GetComponent<WeaponIdentification>();
    }

    // Update is called once per frame
    void Update()
    {
        if (id!=null&&updateInput&&_inputModule.isPressed("Interaction/Fire(click)")) //按下鼠标左键
        {
            pressFire(default);
        }
    }

    void pressFire(InputAction.CallbackContext callbackContext)
    {
        fire.Invoke();
    }

    private void OnDisable()
    {
        if (id.weapon.shootMethod == ShootingMethod.Press)
        {
            _inputModule.UnBindPerformedAction("Interaction/Fire(click)",pressFire);
        }
    }

    public void IniInput()
    {
        if (id.weapon.shootMethod == ShootingMethod.Press)
        {
            _inputModule.BindPerformedAction("Interaction/Fire(click)",pressFire);
        }
        else if (id.weapon.shootMethod == ShootingMethod.PressAndHold)
        {
            updateInput = true;
        }
    }
}
