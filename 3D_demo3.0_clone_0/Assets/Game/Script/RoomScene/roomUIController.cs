using System;
using System.Collections;
using System.Collections.Generic;
using RoomScene;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/**
 * 这个房间ui设计仅测试可用，未来有可能弃用
 */
public class roomUIController : MonoBehaviour
{
    [Header("Room Status")]
    public GameObject roomState_panel;

    [Header("Local player username")] 
    public TMP_InputField userNameInput;

    public TMP_Text userNameText;
    
    [Header("Local player ready state")]
    public TMP_Text readyStateText;

    public Button readyButton;

    public TMP_Text readyButtonText;
    
    [Header("Local player host state")]
    public TMP_Text HostStateText;

    public Button startGameButton;
    
    [Header("Non-Local player UI prefab")]
    public List<GameObject> NonLocalPlayerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        hideStartButton();
        GameManager._events.playerStateUpdateAction += updateLocalPlayerState;
        GameManager._events.allPlayerReadyAction += gameIsReadyToStart;
        GameManager._events.newPlayerEnterAction+=OnNewPlayerEntered;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        // roomState_panel = gameObject.transform.Find("teamStatus").gameObject;
        // userNameInput = gameObject.transform.Find("teamStatus").GetComponent<TMP_InputField>();
        // roomState_panel = gameObject.transform.Find("teamStatus").gameObject;
        // roomState_panel = gameObject.transform.Find("teamStatus").gameObject;
        
    }

    public void addClientInstance(GameObject roomPlayer)
    {
        if (roomPlayer.GetComponent<clientUIController>() != null &&
            roomPlayer.GetComponent<clientUIController>().netId != 100)
        {
            roomPlayer.transform.SetParent(roomState_panel.transform,false);
            roomPlayer.GetComponent<clientUIController>().updateState();
            NonLocalPlayerPrefab.Add(roomPlayer);
            GameManager._events.newPlayerEnterAction.Invoke();
        }
    }

    public void OnNewPlayerEntered()
    {
        foreach (GameObject nonLocal in NonLocalPlayerPrefab)
        {
            nonLocal.GetComponent<clientUIController>().updateState();
        }
    }

    public void changeLocalReadyStatus()
    {
        foreach (roomPlayerInstance roomPlayer in MyNetworkManager.singleton.readyRoomSlot)
        {
            if (roomPlayer.isLocal)
            {
                roomPlayer.CmdChangeReadyState(!roomPlayer.isReady);
            }
        }
    }

    public void changeUserName()
    {
        string name = userNameInput.text;
        foreach (roomPlayerInstance roomPlayer in MyNetworkManager.singleton.readyRoomSlot)
        {
            if (roomPlayer.isLocal)
            {
                roomPlayer.CmdChangeUsername(name);
            }
        }
    }

    //just update the local player ui
    public void updateLocalPlayerState()
    {
        foreach (roomPlayerInstance roomPlayer in MyNetworkManager.singleton.readyRoomSlot)
        {
            if (roomPlayer.isLocal)
            {
                //update user name
                userNameText.text = "<color=#1E67F7>"+roomPlayer.userName+"</color>";
                //update ready state
                readyStateText.text=roomPlayer.isReady ? "<color=#35F617>Ready</color>" : "<color=#EC2B45>Not Ready</color>";
                readyButtonText.text=roomPlayer.isReady ? "Cancel" : "Ready";
                HostStateText.text=roomPlayer.isHost?"<color=#35F617>Yes</color>" : "<color=#EC2B45>No</color>";
                if (roomPlayer.isHost)
                {
                    //todo update when the room player is also a host, like start the game
                }
            }
        }
    }

    /**
     * verify all player in ready state, and verify whether the local player is also host
     */
    public void hostStartGame()
    {
        int totalPlayer = MyNetworkManager.singleton.readyRoomSlot.Count;
        int currentReadyPlayer = 0;
        roomPlayerInstance hostPlayer=null;
        foreach (roomPlayerInstance roomPlayer in MyNetworkManager.singleton.readyRoomSlot)
        {
            if (roomPlayer.isReady)
            {
                currentReadyPlayer += 1;
            }

            if (roomPlayer.isHost&&roomPlayer.isLocal)
            {
                hostPlayer = roomPlayer;
            }
        }
        //verify the number of ready player
        if (totalPlayer == currentReadyPlayer && hostPlayer!=null)
        {
            hostPlayer.CmdStartGame();
        }
    }

    public void gameIsReadyToStart(bool readyOrNot)
    {
        if (readyOrNot)
        {
            showStartButton();
            return;
        }
        hideStartButton();
    }

    public void showStartButton()
    {
        startGameButton.gameObject.SetActive(true);
    }

    public void hideStartButton()
    {
        startGameButton.gameObject.SetActive(false);
    }
    
}
