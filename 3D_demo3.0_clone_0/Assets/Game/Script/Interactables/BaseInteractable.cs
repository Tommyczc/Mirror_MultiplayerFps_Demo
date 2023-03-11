using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider),typeof(NetworkIdentity))]
public class BaseInteractable : NetworkBehaviour
{
    public string displayName;
    public InteractableHandType handType;
    public virtual void Start()
    {
        gameObject.tag = "Interactable";
    }

    public virtual void OnInteract(GameObject fatherObject)
    {
        // to be overriden
    }
    
    public virtual void OffInteract()
    {
        // to be overriden
    }
}

public enum InteractableHandType
{
    Right,
    Both
}
