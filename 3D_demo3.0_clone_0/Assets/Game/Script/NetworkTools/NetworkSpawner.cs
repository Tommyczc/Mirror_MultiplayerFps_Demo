using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class NetworkSpawner : NetworkBehaviour
{
    public GameObject objectSpawn;

    public float highOfFloat;

    public float rotationSpeed;
    // Start is called before the first frame update
    private void OnValidate()
    {
        GetComponent<NetworkIdentity>().serverOnly = true;
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
    void Update()
    {
        
    }

    [Command(requiresAuthority = false)]
    void CmdSpawnObject()
    {
        if (objectSpawn.GetComponent<Rigidbody>() != null)
            objectSpawn.GetComponent<Rigidbody>().isKinematic = true;
        var spawnPosition = transform.position;
        var spawnRotation = transform.rotation;
        GameObject test=Instantiate(objectSpawn,spawnPosition,spawnRotation);
        NetworkServer.Spawn(
            test
        );
    }
}
