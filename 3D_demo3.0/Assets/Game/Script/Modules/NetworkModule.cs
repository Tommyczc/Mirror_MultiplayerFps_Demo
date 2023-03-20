using System;
using System.Collections;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;

public class networkModule : GameModule
{
    private static GameObject networkManagerObject;

    public static MyNetworkManager _MyNetworkManager;

    public static KcpTransport _kcpTransport;

    private void OnValidate()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnRegister()
    {
        iniNetwork();
    }

    public override void OnUnregister()
    {
        //
    }

    public static void changeAddress(int port, string ip = "localhost")
    {
        if (_MyNetworkManager != null)
        {
            _MyNetworkManager.networkAddress = ip;
        }

        if (_kcpTransport != null)
        {
            _kcpTransport.Port = (ushort)port;
        }
    }
    
    // /**
    //  * initial the player prefab in the network sever
    //  */
    // public static GameObject ini_player(string prefabName)
    // {
    //     GameObject player = Resources.Load<GameObject>("Prefabs/Players/"+prefabName);
    //     if (player != null)
    //     {
    //         iniPrefab(player);
    //         return player;
    //     }
    //     else
    //     {
    //         Debug.Log("player prefab cannot found");
    //         return null;
    //     }
    // }
    //
    // public static void iniPrefab(GameObject prefab)
    // {
    //     if (_MyNetworkManager != null && prefab != null)
    //     {
    //         _MyNetworkManager.playerPrefab = prefab;
    //     }
    // }

    public static void spawnPlayerPrefab(string userName,string prefabName)
    {
        if (NetworkClient.isConnected)
            NetworkClient.ready = false;
            NetworkClient.Send(new Messages.CreatePlayerMessage()
            {
                userName = userName,
                PrefabName = prefabName,
                spawnMethod = Messages.SpawnMethod.spawn,
                deleteOldPlayer = false
            });
    }

    public static void replacePlayerPrefab(string userName,string prefabName,bool deleteOldPlayer=false)
    {
        if (NetworkClient.isConnected)
            NetworkClient.ready = false;
            NetworkClient.Send(new Messages.CreatePlayerMessage()
            {
                userName = userName,
                PrefabName = prefabName,
                spawnMethod = Messages.SpawnMethod.replace,
                deleteOldPlayer= deleteOldPlayer,
            });
    }

    public static void replaceExistPlayerPrefab(string sceneName,bool deleteOldPlayer=false)
    {
        if (NetworkClient.isConnected)
            NetworkClient.ready = false;
        NetworkClient.Send(new Messages.CreatePlayerMessage()
        {
            userName = "Unknown",
            PrefabName = sceneName,
            spawnMethod = Messages.SpawnMethod.replaceExitObject,
            deleteOldPlayer= deleteOldPlayer,
        });
    }

    private void iniNetwork()
    {
        networkManagerObject = Instantiate(Resources.Load<GameObject>("Defaults/network"));
        networkManagerObject.name = "networkManagerObject";
        _MyNetworkManager = networkManagerObject.GetComponent<MyNetworkManager>();
        if (_MyNetworkManager != null)
        {
            _kcpTransport = _MyNetworkManager.GetComponent<KcpTransport>();
        }
        networkManagerObject.transform.SetParent(transform,false);
    }
}
