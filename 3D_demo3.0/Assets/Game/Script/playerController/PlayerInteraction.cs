using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private TMP_Text tooltip;
    
    [Header("DEBUG")]
    [SerializeField] private List<GameObject> activeInteractables;
    

    private InputModule _input;
    private int _currentIndex;
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] public GameObject interactionStartPoint;

    public Transform rightHand;
    public Transform middleHand;
    public Transform leftHand;

    
    private void Awake()
    {
        activeInteractables = new List<GameObject>();
        _currentIndex = -1;
        _input = App.Modules.Get<InputModule>();
    }

    private void Start()
    {
        if (!isLocalPlayer) return;
        _input.BindPerformedAction("Interaction/Interact", OnInteract);
        _input.BindPerformedAction("Interaction/Release", OnRelease);
        _input.BindPerformedAction("Interaction/ChangeTarget", OnChangeTarget);
        
        tooltip.text = string.Empty;
    }
    
    private void Update()
    {
        if (!isLocalPlayer) return;
        RaycastHit hit;
        Ray ray = new Ray(interactionStartPoint.transform.position, interactionStartPoint.transform.forward);
        bool isHit = Physics.Raycast(ray, out hit, maxDistance, 1 << 0, QueryTriggerInteraction.Collide);

        if (isHit)
        {
            if (hit.transform.CompareTag("Interactable")&&!activeInteractables.Contains(hit.transform.gameObject))
            {
                activeInteractables.Add(hit.transform.gameObject);
                _currentIndex = activeInteractables.Count - 1;
                UpdateTooltip();
            }
        }
        else
        {
            activeInteractables.Clear();
            _currentIndex = -1;
            UpdateTooltip();
        }
    }
    
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (_currentIndex == -1) return;
        activeInteractables[_currentIndex].GetComponent<BaseInteractable>().OnInteract(gameObject);
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        //Globals.Instance.playerHands.ReleaseItemInHand();
    }

    private void OnChangeTarget(InputAction.CallbackContext ctx)
    {
        if (activeInteractables.Count >= 2)
        {
            _currentIndex = (_currentIndex + 1) % activeInteractables.Count;
            UpdateTooltip();
        }
    }

    private void UpdateTooltip()
    {
        //todo: more precise and action-relative tooltip
        
        if (_currentIndex == -1)
        {
            tooltip.text = string.Empty;
            return;
        }

        string displayName = activeInteractables[_currentIndex].GetComponent<BaseInteractable>().displayName;
        tooltip.text = $"Press <color=#EBE800>E</color> to {displayName}\n";

        if (activeInteractables.Count >= 2)
        {
            tooltip.text += "Press <color=#EBE800>TAB</color> to switch operation";
        }
    }
    
}
