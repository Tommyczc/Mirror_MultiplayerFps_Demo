using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class PlayerHands : MonoBehaviour
{

    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform bothHands;

    [SerializeField] private Transform releasedItems;


    public bool isCarting=false;
    public bool leftHandFree;
    public bool rightHandFree;

    public GrabInteractable leftHandHolding;
    public GrabInteractable rightHandHolding;
    //public Cart cartHolding;

    
    private void Awake()
    {
        leftHandFree = true;
        rightHandFree = true;
    }

    public void GrabItem(GrabInteractable target)
    {
        switch (target.handType)
        {
            case InteractableHandType.Right:
                if (!rightHandFree) return;
                target.transform.SetParent(rightHand, true);
                target.transform.position = rightHand.position;
                break;
            case InteractableHandType.Both:
                if (!(leftHandFree && rightHandFree)) return;
                target.transform.SetParent(bothHands, true);
                target.transform.position = bothHands.position;
                leftHandFree = false;
                break;
        }
        target.tag = "OnGrab";
        rightHandHolding = target;
        rightHandFree = false;
    }

    public void GrabPad()
    {
        leftHandFree = false;
        //todo: Actually hold the pad on hand
    }
    
    public void ReleaseItemInHand()
    {
        if (rightHandFree) return;
        //if (isCarting) { ReleaseCart(); }
        rightHandHolding.transform.SetParent(releasedItems, true);
        rightHandHolding.tag = "Interactable";
        rightHandHolding.GetComponent<Rigidbody>().isKinematic = false;
        rightHandFree = true;
        if (rightHandHolding.handType == InteractableHandType.Both) leftHandFree = true;
        rightHandHolding = null;
    }

#if false
#region Carting

    public void GrabCart(Cart cart)
    {
        leftHandFree = rightHandFree = false;
        cart.tag = "OnGrab";
        cart.transform.parent.position = bothHands.transform.position;
        cart.transform.parent.gameObject.transform.SetParent(bothHands.transform);
        cart.transform.parent.localRotation = Quaternion.Euler(0, -180, 0);
        cartHolding = cart;
        isCarting = true;
        
        
        //Debug.Log("on cart");

    }

    public void ReleaseCart()
    {
        Cart cart=cartHolding;
        cart.transform.parent.gameObject.transform.SetParent(null);
        isCarting = false;
        cart.tag = "Interactable";
        leftHandFree = rightHandFree = true;
        cartHolding = null;
        //Debug.Log("off cart");
    }

#endregion
#endif
}
