using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.Serialization;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class MyNetworkManager : NetworkManager
{
    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new MyNetworkManager singleton { get; private set; }
    //仅server/host端可用
    public List<Messages.RoomInstanceMessage> ReadyRoom=new List<Messages.RoomInstanceMessage>();
    //每个客户端都会同步
    [FormerlySerializedAs("Instance Slot")] 
    public List<roomPlayerInstance> readyRoomSlot=new List<roomPlayerInstance>();
    public List<PlayerInformationSyn> gameRoomSlot = new List<PlayerInformationSyn>();
    
    [HideInInspector]
    public roomPlayerInstance currentRoomInstance;
    public PlayerInformationSyn currentGameInstance;
    
    [Header("Additional Scene Management")]
    [Scene]
    public string roomScene;
    [Scene] 
    public string startScene;

    [Header("Ready State")] 
    private bool _allPlayersReady;
    public bool allPlayersReady { set; get; }

    [Header("Player number (not connection number)")]
    public int maxNumOfPlayer=4;
    public int miniNumOfPlayer = 1;

    [Header("player prefab group")] 
    public List<GameObject> playerPrefabGroup=new List<GameObject>();
    
    #region Unity Callbacks

    public override void OnValidate()
    {
        base.OnValidate();
        
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Start()
    {
        singleton = this;
        base.Start();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
    {
        
        base.LateUpdate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    #region Start & Stop

    /// <summary>
    /// Set the frame rate for a headless server.
    /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
    /// </summary>
    public override void ConfigureHeadlessFrameRate()
    {
        base.ConfigureHeadlessFrameRate();
    }

    /// <summary>
    /// called when quitting the application by closing the window / pressing stop in the editor
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        //base.ServerChangeScene(newSceneName);
        if (string.IsNullOrWhiteSpace(newSceneName))
        {
            Debug.LogError("ServerChangeScene empty scene name");
            return;
        }

        if (NetworkServer.isLoadingScene && newSceneName == networkSceneName)
        {
            Debug.LogError($"Scene change is already in progress for {newSceneName}");
            return;
        }

        // Debug.Log($"ServerChangeScene {newSceneName}");
        NetworkServer.SetAllClientsNotReady();
        networkSceneName = newSceneName;

        // Let server prepare for scene change
        OnServerChangeScene(newSceneName);

        // set server flag to stop processing messages while changing scenes
        // it will be re-enabled in FinishLoadScene.
        NetworkServer.isLoadingScene = true;

        //if only server, just load the scene
         if (NetworkServer.active && !NetworkClient.active)
         {
             loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
         }
         //if host, load the scene asyn
         else if(NetworkServer.active && NetworkClient.active)
         {
             App.Modules.Get<SceneModule>().asynServerScene(newSceneName);
         }
         
         if (NetworkServer.active)
         {
             Debug.Log($"Sending scene message to client: {newSceneName}");
             // notify all clients about the new scene
             NetworkServer.SendToAll(new SceneMessage
             {
                 sceneName = newSceneName,
                 customHandling = true
             });
         }
         startPositionIndex = 0;
         startPositions.Clear();
    }

    /// <summary>
    /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    public override void OnServerChangeScene(string newSceneName)
    {
    }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName)
    {
    }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    /// 附加：假如需要取消动态加载（从服务端发送的sceneMessage），请去NetworkManger的 OnServerAuthenticated（） 把scenemessage 的customHandling设为false
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        //server already update the scene
          if (NetworkServer.active) return;
         
          Debug.Log("try to load a scene: "+newSceneName+" operation:"+sceneOperation+" customHandling:"+customHandling);
          switch (sceneOperation)
              {
                  // TODO 仅重写 normal 类型
                  case SceneOperation.Normal:
                      Debug.Log($"client scene {newSceneName} is loading");
                      //loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
                      App.Modules.Get<SceneModule>().asynServerScene(newSceneName);
                      break;
                  case SceneOperation.LoadAdditive:
                      // Ensure additive scene is not already loaded on client by name or path
                      // since we don't know which was passed in the Scene message
                      if (!SceneManager.GetSceneByName(newSceneName).IsValid() && !SceneManager.GetSceneByPath(newSceneName).IsValid())
                          loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
                      else
                      {
                          Debug.LogWarning($"Scene {newSceneName} is already loaded");
         
                          // Reset the flag that we disabled before entering this switch
                          NetworkClient.isLoadingScene = false;
                      }
                      break;
                  case SceneOperation.UnloadAdditive:
                      // Ensure additive scene is actually loaded on client by name or path
                      // since we don't know which was passed in the Scene message
                      if (SceneManager.GetSceneByName(newSceneName).IsValid() || SceneManager.GetSceneByPath(newSceneName).IsValid())
                          loadingSceneAsync = SceneManager.UnloadSceneAsync(newSceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                      else
                      {
                          Debug.LogWarning($"Cannot unload {newSceneName} with UnloadAdditive operation");
         
                          // Reset the flag that we disabled before entering this switch
                          NetworkClient.isLoadingScene = false;
                      }
                      break;
              }
         
              // don't change the client's current networkSceneName when loading additive scene content
              if (sceneOperation == SceneOperation.Normal)
                  networkSceneName = newSceneName;
    }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
        // todo: if player enter the game after room scene, disconnect and send "game has been start" error message
        if (!IsSceneActive(roomScene))
        {
            //todo: send error message
            conn.Disconnect();
            return;
        }

        if (ReadyRoom.Count >= maxNumOfPlayer)
        {
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);
    }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        Debug.Log($"connection pickUp {conn.connectionId} is ready, from server");
        if (conn != null && conn.identity != null)
        {
            Debug.Log($"connection {conn.connectionId} is ready, prefab name {conn.identity.gameObject.name}");
            if (IsSceneActive(roomScene))
            {
                GameObject player = conn.identity.gameObject;
                if (player != null && player.GetComponent<roomPlayerInstance>() != null)
                {
                     SceneLoadedForPlayer(conn,player);
                }
            }
        }
    }

    void SceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        if (ReadyRoom.Count == 0)
        {
            //todo 假设第一个进来的人是房主（考虑到有纯服务器模式，只能设定第一个进来的人是房主/主机）
            if (roomPlayer.GetComponent<roomPlayerInstance>() != null)
                roomPlayer.GetComponent<roomPlayerInstance>().isHost = true;
                Debug.Log($"connection {conn.connectionId} is host");
        }
        Debug.LogWarning($"player name {"player"+readyRoomSlot.Count}");
        roomPlayer.GetComponent<roomPlayerInstance>().userName = "player " + readyRoomSlot.Count;
        
        checkPlayerReadyState();
        
        Messages.RoomInstanceMessage roomInstance;
        roomInstance.conn=conn;
        roomInstance.roomPlayer = gameObject;
        ReadyRoom.Add(roomInstance);
        
        Debug.Log($"scene name: {SceneManager.GetActiveScene().name}, room prefab {roomPlayer.name} is added");
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        if (IsSceneActive(roomScene))
        {
            foreach (Messages.RoomInstanceMessage roomInstance in ReadyRoom)
            {
                if (roomInstance.conn.connectionId == conn.connectionId)
                    ReadyRoom.Remove(roomInstance);
                    break;
            }

            foreach (roomPlayerInstance client in readyRoomSlot)
            {
                if (conn.connectionId == client.connectionToClient.connectionId)
                {
                    readyRoomSlot.Remove(client);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Called on server when transport raises an exception.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message) { }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    public override void OnClientDisconnect()
    {
        //NetworkClient.Disconnect();
        Debug.Log("OnClientDisconnect");
        UIModule.showErrorUI("Server Disconnection", "Lost connection from server, go to menu in 3 seconds!!", 2.5f);
        
        readyRoomSlot.Clear();
        gameRoomSlot.Clear();
        currentRoomInstance = null;
        currentGameInstance = null;
        
        if (NetworkServer.active || IsSceneActive(startScene)) return;
        StartCoroutine(OnClientDisconnectLoadScene());
    }

    private IEnumerator OnClientDisconnectLoadScene()
    {
        yield return new WaitForSeconds(3f);
        App.Modules.Get<SceneModule>().asynServerScene(startScene,false);
    }

    /// <summary>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// <para>This is commonly used when switching scenes.</para>
    /// </summary>
    public override void OnClientNotReady() { }

    /// <summary>
    /// Called on client when transport raises an exception.</summary>
    /// </summary>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnClientError(TransportError transportError, string message) { }

    #endregion

    #region Start & Stop Callbacks

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost() { }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<Messages.CreatePlayerMessage>(OnCreateCharacter);
    }

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        registerPlayerPrefab();
    }

    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost()
    {
        readyRoomSlot.Clear();
        gameRoomSlot.Clear();
        ReadyRoom.Clear();
        currentRoomInstance = null;
        currentGameInstance = null;
    }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer()
    {
        readyRoomSlot.Clear();
        gameRoomSlot.Clear();
    }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() { }

    #endregion
    
    void OnCreateCharacter(NetworkConnectionToClient conn, Messages.CreatePlayerMessage message)
    {
        GameObject player=null;
        if (message.spawnMethod != Messages.SpawnMethod.replaceExitObject)
        {
            //GameObject playerPref = Resources.Load<GameObject>("Prefabs/Players/Player")
            Transform startPos = GetStartPosition();
            player = startPos != null && message.PrefabName != null
                ? Instantiate(Resources.Load<GameObject>(message.PrefabName), startPos.position,
                    startPos.rotation)
                : Instantiate(Resources.Load<GameObject>(message.PrefabName));

            if (player == null)
            {
                Debug.Log("cannot find player prefab");
                player = startPos != null
                    ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                    : Instantiate(player);
            }
        }

        //player.name = $"{player.name} [connId={conn.connectionId}]";
        GameObject oldPlayer = null;
        try
        {
            oldPlayer = conn.identity.gameObject;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Catch an exception: "+e.Message);
        }
        finally
        {
            if (oldPlayer==null&&(message.spawnMethod == Messages.SpawnMethod.replace ||
                message.spawnMethod == Messages.SpawnMethod.replaceExitObject))
            {
                message.spawnMethod = Messages.SpawnMethod.spawn;
            }
        }

        switch (message.spawnMethod)
        {
            case Messages.SpawnMethod.spawn:
                Debug.Log($"connection pickUp: {conn.connectionId} spawn player prefab {player.name}");
                if(player!=null)
                    NetworkServer.AddPlayerForConnection(conn, player);
                break;
            
            case Messages.SpawnMethod.replace:
                Debug.Log($"connection pickUp: {conn.connectionId} replace player prefab {player.name}");
                if(oldPlayer!=null)
                    NetworkServer.ReplacePlayerForConnection(conn, player, true);
                if(message.deleteOldPlayer)
                    NetworkServer.Destroy(oldPlayer);
                break;
            
            case Messages.SpawnMethod.replaceExitObject:
                Debug.Log($"connection pickUp: {conn.connectionId} replace exist player prefab {message.PrefabName}");
                if (message.PrefabName == "room" || currentRoomInstance != null)
                {
                    NetworkServer.ReplacePlayerForConnection(conn, currentRoomInstance.gameObject, true);
                }
                else if (message.PrefabName == "gamePlayer" || currentGameInstance != null)
                {
                    NetworkServer.ReplacePlayerForConnection(conn, currentGameInstance.gameObject, true);
                }
                
                if(message.deleteOldPlayer)
                    NetworkServer.Destroy(oldPlayer);
                break;
        }
    }


    void registerPlayerPrefab()
    {
        
        //register player prefab
        // GameObject[] playerGroup = Resources.LoadAll<GameObject>("Prefabs/Players");
        // foreach (GameObject player in playerGroup)
        // {
        //     if (player.GetComponent<NetworkIdentity>() != null)
        //     {
        //         NetworkClient.RegisterPrefab(player);
        //     }
        // }
        //
        // //register room prefab
        // GameObject roomPlayer = Resources.Load<GameObject>("Prefabs/RoomPlayer/roomInstance");
        // if (roomPlayer.GetComponent<NetworkIdentity>() != null)
        // {
        //     NetworkClient.RegisterPrefab(roomPlayer);
        // }

        foreach (GameObject playerPrefab in playerPrefabGroup)
        {
            if (playerPrefab.GetComponent<NetworkIdentity>() != null)
            {
                if (spawnPrefabs.Contains(playerPrefab))
                {
                    spawnPrefabs.Remove(playerPrefab);
                }
                else
                {
                    NetworkClient.RegisterPrefab(playerPrefab);
                }
            }
        }
    }
    
    public void checkPlayerReadyState()
    {
        int currentPlayerNum = readyRoomSlot.Count;
        int readyPlayerNum = 0;
        roomPlayerInstance hostPlayer=null;
        NetworkConnection hostConnection=null;
        Debug.LogWarning($"current player number: {currentPlayerNum}");
        foreach (roomPlayerInstance roomPlayer in readyRoomSlot)
        {
            if (roomPlayer.isReady)
            {
                readyPlayerNum++;
            }
            if (roomPlayer.isHost)
            {
                hostPlayer = roomPlayer;
                hostConnection = roomPlayer.connectionToServer;
            }
            
        }
        
        if(hostPlayer==null)return;
        
        if (readyPlayerNum == currentPlayerNum && currentPlayerNum >= miniNumOfPlayer)
        {
            hostPlayer.RpcAllClientReady(hostPlayer.connectionToClient,true);
        }
        else
        {
            hostPlayer.RpcAllClientReady(hostPlayer.connectionToClient,false);
        }
    }
}
