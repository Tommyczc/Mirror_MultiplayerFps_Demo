using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Messages
{
    public struct CreatePlayerMessage : NetworkMessage
    {
        public string userName;
        public string PrefabName;
        public SpawnMethod spawnMethod;
        public bool deleteOldPlayer;
    }
    
    public enum SpawnMethod
    {
        spawn,
        replace,
        replaceExitObject,
    }

    public struct RoomInstanceMessage
    {
        public NetworkConnectionToClient conn;
        public GameObject roomPlayer;
    }
}
