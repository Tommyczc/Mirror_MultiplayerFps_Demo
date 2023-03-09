using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EventModule : GameModule
{
    private Dictionary<string, Action<object, object>> _events = new();
    
    public override void OnRegister()
    {
    }

    public void Listen(string id, Action<object, object> action)
    {
        if (action == null)
        {
            Debug.Log("Null listening");
            return;
        }

        if (_events.ContainsKey(id))
        {
            Delegate[] actionsList = _events[id].GetInvocationList();
            if (Array.Exists(actionsList, s => s == (Delegate) action))
            {
                Debug.Log("Duplicated listening");
            }
            else
            {
                _events[id] += action;
            }
        }
        else
        {
            _events.Add(id, action);
        }
    }

    public void Invoke(string id, object args = null)
    {
        if (_events.ContainsKey(id))
        {
            _events[id].Invoke(this, args);
        }
    }

    public void Remove(string id, Action<object, object> action)
    {
        if (action == null)
        {
            Debug.Log("Null removal");
            return;
        }
        
        if (_events.ContainsKey(id))
        {
            Delegate[] actionList = _events[id].GetInvocationList();
            if (Array.Exists(actionList, s => s == (Delegate) action))
            {
                _events[id] -= action;
                if (_events[id] == null)
                {
                    _events.Remove(id);
                }
            }
            else
            {
                Debug.Log("No such listening");
            }
        }
        else
        {
            Debug.Log("No such event");
        }
    }

    public override void OnUnregister()
    {
        // 
    }
    
    public void RemoveAll(string id) {
        if (_events.ContainsKey(id)) {
            _events.Remove(id);
        }
    }
}
