using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Modules
{
    public Modules()
    {
        Init();
    }

    private Dictionary<string, GameModule> _modules = new Dictionary<string, GameModule>();
    private GameObject _gameObject;

    /// <summary>
    /// Create game manager object on init.
    /// </summary>
    private void Init()
    {
        if (_gameObject == null)
        {
            _gameObject = new GameObject("Modules");
            Object.DontDestroyOnLoad(_gameObject);
        }
    }

    /// <summary>
    /// Get a module.
    /// </summary>
    public T Get<T>() where T : GameModule
    {
        string key = typeof(T).Name;
        if (!_modules.ContainsKey(key))
        {
            Debug.Log($"{key} is not registered.");
        }

        return (T) _modules[key];
    }

    /// <summary>
    /// Register a module.
    /// Will trigger OnRegister() on module.
    /// </summary>
    public void Register<T>() where T : GameModule
    {
        string key = typeof(T).Name;
        if (_modules.ContainsKey(key))
        {
            Debug.Log($"{key} already registered.");
            return;
        }

        GameObject go = new GameObject(key);
        T module = go.AddComponent<T>();
        go. transform.SetParent(_gameObject.transform);
        _modules.Add(key, module);

        module.OnRegister();
    }

    /// <summary>
    /// Unregister a module
    /// Will trigger OnUnregister() on module.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Unregister<T>() where T : GameModule
    {
        string key = typeof(T).Name;
        if (!_modules.ContainsKey(key))
        {
            Debug.Log($"{key} is not registered.");
            return;
        }

        GameModule module = _modules[key];
        Object.Destroy(module.gameObject);
        module.OnUnregister();
        
        _modules.Remove(key);
    }
}