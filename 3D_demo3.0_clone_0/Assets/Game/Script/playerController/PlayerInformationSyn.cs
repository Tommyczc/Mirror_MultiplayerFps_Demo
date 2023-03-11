using System;
using Mirror;
using TMPro;
using UnityEngine;

public class PlayerInformationSyn:NetworkBehaviour,IDamageable
{
    [HideInInspector]
    public PlayerMovement _playerMovement;

    public WeaponController _weaponController;

    [Header("visible component")] 
    public TMP_Text userNameText;
    

    [SyncVar(hook = nameof(onUsernameChanged))]
    public string userName;

    private bool isHost;
    void Start()
    {
        MyNetworkManager.singleton.gameRoomSlot.Add(this);
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            if(NetworkClient.localPlayer.gameObject!=null)
                userNameText.transform.rotation = Quaternion.LookRotation(userNameText.transform.position - NetworkClient.localPlayer.gameObject.transform.position);
        }
    }

    public override void OnStartLocalPlayer()
    {
        MyNetworkManager.singleton.currentGameInstance = this;
        if (MyNetworkManager.singleton.currentRoomInstance != null)
        {
            CmdChangeUsername(MyNetworkManager.singleton.currentRoomInstance.userName);
        }
    }
    
    void onUsernameChanged(string oldName,string newName)
    {
        Debug.Log($"Name of game player is changed: {newName}");
        if (userNameText != null)
        {
            userNameText.text = newName;
        }
    }
    
    
    [Command]
    private void CmdChangeUsername(string userNameUpdate)
    {
        userName=userNameUpdate;
    }

    [Command]
    public void Damage(float damage)
    {
        Debug.Log($"damage : {damage}");
    }
}
