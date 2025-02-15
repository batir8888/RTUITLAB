using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance { get; private set; }
    
    private readonly Dictionary<Type, object> _services = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterService<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public T GetService<T>()
    {
        if (_services.TryGetValue(typeof(T), out object service))
        {
            return (T)service;
        }
        Debug.LogError($"Сервис {typeof(T)} не зарегистрирован");
        return default;
    }

    public void UnregisterService<T>()
    {
        _services.Remove(typeof(T));
    } 
}
