using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInformationSyn:CharacterHealthState
{
    [Header("Player component")]
    [HideInInspector]
    public PlayerMovement _playerMovement;
    public WeaponController _weaponController;
    public GameObject directionComponent;
    public bool directionSynced;//同步血条ui的位置，对准当前的local player

    [Header("UI")] 
    public TMP_Text userNameText;
    public GameObject sliderScreenObject;
    private Slider healthScreenSlider;
    
    [SyncVar(hook = nameof(onUsernameChanged))]
    private string userName;
    private bool isHost;
    
    [Header("Customized")]
    public bool showScreenSpaceHealthBar;
    
    public override void Start()
    {
        base.Start();
        MyNetworkManager.singleton.gameRoomSlot.Add(this);
        if (showScreenSpaceHealthBar&&sliderScreenObject!=null)
        {
            healthScreenSlider = sliderScreenObject.GetComponent<Slider>() != null
                ? sliderScreenObject.GetComponent<Slider>()
                : null;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            if (!directionSynced) return;
            if(NetworkClient.localPlayer.gameObject!=null)
                directionComponent.transform.rotation = Quaternion.LookRotation(directionComponent.transform.position - NetworkClient.localPlayer.gameObject.transform.position);
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

    public override void onDamaged()
    {
        base.onDamaged();
        if (showScreenSpaceHealthBar&&healthScreenSlider!=null)
        {
            float ratio = (float)Math.Round(currentHealth / Health,2);
            healthScreenSlider.value = healthScreenSlider.maxValue * ratio;
        }
    }

    [Command]
    private void CmdChangeUsername(string userNameUpdate)
    {
        userName=userNameUpdate;
    }

    // [Command]
    // public void Damage(float damage)
    // {
    //     Debug.Log($"damage : {damage}");
    // }
}
