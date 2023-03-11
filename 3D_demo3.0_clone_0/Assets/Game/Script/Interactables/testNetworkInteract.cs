using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class testNetworkInteract : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public override void OnStartClient()
    {
        
    }
    
    public override void OnStartServer()
    {
        NetworkClient.Ready();
        CmdcreateTestObject();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    [Command(requiresAuthority = false)]
    void CmdcreateTestObject()
    {
        GameObject offlineTest=Resources.Load<GameObject>("Prefabs/Other/P_ContainerM_Cylinder_1 (11) Variant");
        var spawnPosition = transform.position;
        var spawnRotation = transform.rotation;
        GameObject test=Instantiate(offlineTest,spawnPosition,spawnRotation);
        NetworkServer.Spawn(
            test
        );
    }
}
