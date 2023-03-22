using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace welcomeScene
{
    public class GameManager : MonoBehaviour
    {
        private bool isLoaded;
        // Start is called before the first frame update
        void Start()
        {
            if (Globals.isServer)
            {
                MyNetworkManager.singleton.ServerChangeScene("Main");
                return;
            }

            
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UIModule.showMenu();
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

