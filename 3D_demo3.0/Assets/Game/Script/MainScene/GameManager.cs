using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace MainScene
{
    public class GameManager : MonoBehaviour
    {
        // public override void OnStartLocalPlayer()
        // {
        //     
        // }

        // Start is called before the first frame update
        void Start()
        {
            if (!NetworkClient.isConnected) return;
            Debug.Log(networkModule._MyNetworkManager.GetStartPosition().position);
            
            networkModule.replacePlayerPrefab("lala","Prefabs/Players/Player");
        }

            // Update is called once per frame
        void Update()
        {
        
        }

        void Awake()
        {
            
        }
    }
}

