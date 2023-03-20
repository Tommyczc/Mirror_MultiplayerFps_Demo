using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * 更新其他客户端的ui，通过netid进行绑定
 */
public class clientUIController : MonoBehaviour
{
    [Header("user name")] public TMP_Text userNameText;
    [Header("ready state")] public TMP_Text readyStateText;
    [Header("is host")]public TMP_Text hostStateText;
    [Header("connection")] public uint netId=100;
    
    // Start is called before the first frame update
    void Start()
    {
        readyStateText.text = "<color=#EC2B45>Not Ready</color>";
        hostStateText.text="<color=#EC2B45>No</color>";
        updateState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateState()
    {
        if (netId == 100) return;
        foreach (roomPlayerInstance roomPlayer in MyNetworkManager.singleton.readyRoomSlot)
        {
            if (roomPlayer.netId==netId)
            {
                //update user name
                userNameText.text = "Player "+"<color=#1E67F7>"+roomPlayer.userName+"</color>";
                //update ready state
                readyStateText.text=roomPlayer.isReady ? "<color=#35F617>Ready</color>" : "<color=#EC2B45>Not Ready</color>";
                //update host state
                hostStateText.text=roomPlayer.isHost ? "<color=#35F617>Yes</color>" : "<color=#EC2B45>No</color>";
            }
        }
    }
}
