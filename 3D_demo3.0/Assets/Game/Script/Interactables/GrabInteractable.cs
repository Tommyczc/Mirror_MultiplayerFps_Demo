using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabInteractable : BaseInteractable
{
    public override void OnInteract(GameObject fatherObject)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        Globals.Instance.playerHands.GrabItem(this);
    }
}
