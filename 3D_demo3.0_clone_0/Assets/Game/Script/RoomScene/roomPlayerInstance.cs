using Mirror;
using RoomScene;
using UnityEngine;

public class roomPlayerInstance : NetworkBehaviour
{
    [Header("sync var value")]
    [SyncVar(hook=nameof(onReadyStateUpdated))]
    public bool isReady;

    [SyncVar(hook = nameof(onUsernameUpdated))]
    public string userName;

    [SyncVar(hook = nameof(onIshostUpdated))]
    public bool isHost;

    [Header("Local value")]
    public bool isLocal;
    
    //only for non-local player
    public GameObject roomUI;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        MyNetworkManager.singleton.readyRoomSlot.Add(this);
        onGui();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        
    }

    public void hello()
    {
        Debug.Log("hello");
    }

    public override void OnStartLocalPlayer()
    {
        isLocal = true;
        MyNetworkManager.singleton.currentRoomInstance = this;
        //CmdChangeReadyState(false);
    }

    #region syncvar field
    public void onReadyStateUpdated(bool oldValue, bool newValue )
    {
        Debug.Log($"connection {netId.ToString()}: ready state change {oldValue}->{newValue}");
        if (isLocalPlayer)
        {
            //GameManager.updateLocalRoomUi();
            updateLocalRoomUi();
        }
        else
        {
            updateClientUI();
        }
    }

    public void onUsernameUpdated(string oldUsername,string newUsername)
    {
        Debug.Log($"connection {netId.ToString()}: username change {oldUsername}->{newUsername}");
        if (isLocalPlayer)
        {
            updateLocalRoomUi();
        }
        else
        {
            updateClientUI();
        }
    }

    public void onIshostUpdated(bool oldValue, bool newValue)
    {
        Debug.Log($"connection {netId.ToString()}: host status change {oldValue}->{newValue}");
        if (isLocalPlayer)
        {
            updateLocalRoomUi();
        }
        else
        {
            updateClientUI();
        }
    }

    #endregion

    #region Command field

    [Command]
    public void CmdChangeReadyState(bool ready)
    {
        isReady = ready;
        if (MyNetworkManager.singleton != null)
        {
            MyNetworkManager.singleton.checkPlayerReadyState();
        }
    }

    [Command]
    public void CmdChangeUsername(string name)
    {
        userName = name;
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isHost) return;
        
        if (MyNetworkManager.singleton != null)
        {
            MyNetworkManager.singleton.ServerChangeScene("Main");
        }
    }

    #endregion

    #region client rpc

    [TargetRpc]
    public void RpcAllClientReady(NetworkConnection target, bool readyState)
    {
        if (isHost&isLocal)
        {
            GameManager._events.allPlayerReadyAction.Invoke(readyState);
        }
    }

    #endregion
    
    private void OnDisable()
    {
        // NetworkServer.Destroy(this.gameObject);
        //todo delete this player prefab, and update ui
        if (isLocalPlayer)
        {
            // NetworkServer.Destroy(this.gameObject);
        }
        else
        {
            //MyNetworkManager.singleton.readyRoomSlot.Remove(this);
            if (roomUI != null)
            {
                Destroy(roomUI);
            }
        }
    }

    //only for non-local client
    public void onGui()
    {
        if (isLocalPlayer)
        {
            updateLocalRoomUi();
        }
        else
        {
            Debug.Log("non-local player only");
            roomUI = Instantiate(Resources.Load<GameObject>("Prefabs/UI/roomUI/clientUI"));
            
            if (roomUI == null)
            {
                Debug.LogWarning("client ui is null");
            }

            roomUI.GetComponent<clientUIController>().netId = netId;
            GameManager.addOtherClientUI(roomUI);
        }
    }

    void updateClientUI()
    {
        if (isLocalPlayer) return;
        if (roomUI != null)
        {
            roomUI.GetComponent<clientUIController>().updateState();
        }
    }

    void updateLocalRoomUi()
    {
        GameManager._events.playerStateUpdateAction.Invoke();
    }

}
