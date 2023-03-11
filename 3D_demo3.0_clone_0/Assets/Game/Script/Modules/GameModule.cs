using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameModule : MonoBehaviour
{
    public abstract void OnRegister();
    public abstract void OnUnregister();
}
