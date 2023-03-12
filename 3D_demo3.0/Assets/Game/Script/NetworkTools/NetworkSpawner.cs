using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class NetworkSpawner : NetworkBehaviour
{
    public GameObject objectSpawn;

    public float highOfFloat;

    public float rotationSpeed;

    [HideInInspector]
    [SyncVar]
    public bool isInstantiated=false;
    
    // Start is called before the first frame update
    public virtual void OnValidate()
    {
        //GetComponent<NetworkIdentity>().serverOnly = true;
        // if (!MyNetworkManager.singleton.spawnPrefabs.Contains(objectSpawn))
        // {
        //     MyNetworkManager.singleton.spawnPrefabs.Add(objectSpawn);
        // }
    }

    void Start()
    {
        
    }

    public override void OnStartServer()
    {
        if (objectSpawn.GetComponent<NetworkIdentity>() != null &&
            MyNetworkManager.singleton.spawnPrefabs.Contains(objectSpawn))
        {
            //NetworkClient.Ready();
            CmdSpawnObject();
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public virtual void Awake()
    {
        
    }

    [Command(requiresAuthority = false)]
    void CmdSpawnObject()
    {
        if (objectSpawn.GetComponent<Rigidbody>() != null)
            objectSpawn.GetComponent<Rigidbody>().isKinematic = true;
        var spawnPosition = transform.position;
        var spawnRotation = transform.rotation;
        
        GameObject test = null;
        if (!isInstantiated)
        {
            test = Instantiate(objectSpawn, spawnPosition, spawnRotation);
        }
        else
        {
            test = objectSpawn;
        }

        NetworkServer.Spawn(
            test
        );
    }
}
