using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace RoomScene
{
    public class GameManager : MonoBehaviour
    {
        // public GameObject theRoomUI;
        public static roomUIController roomUI;

        public class events {
        public UnityAction playerStateUpdateAction;
        public UnityAction<bool> allPlayerReadyAction;
        public UnityAction newPlayerEnterAction;
        }

        public static events _events;

    // Start is called before the first frame update
        void Start()
        {
            if (NetworkClient.isConnected)
            {
                if (MyNetworkManager.singleton.currentRoomInstance == null)
                {
                    networkModule.spawnPlayerPrefab("Unknown", "Prefabs/RoomPlayer/roomInstance");
                }
                else
                {
                    networkModule.replaceExistPlayerPrefab("room");
                }
            }
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }

        private void OnDisable()
        {
            UIModule.DestroyUI(roomUI.gameObject);
        }

        private void Awake()
        {
            if (roomUI == null)
            {
                GameObject roomObject=UIModule.ShowUI(Resources.Load<GameObject>("Prefabs/UI/roomUI/RoomPanel"));
                roomUI = roomObject.GetComponent<roomUIController>();
                _events = new events();
            }
        }

        public static void addOtherClientUI(GameObject clientUI)
        {
            roomUI.addClientInstance(clientUI);
        }
        
    }
}

