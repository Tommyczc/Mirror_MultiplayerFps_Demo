using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Michsky.UI.ModernUIPack;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class startMenu : MonoBehaviour
{
    [Header("online server option")]
    public static string server_ip="";
    public static string server_port = "";

    [Header("local")] 
    public InputField ip_input;
    public InputField port_input;

    private InputModule _inputModule;

    private void Start()
    {
        
    }

    public void joinLocal()
    {
        int port = int.Parse(port_input.text);
        string ip = ip_input.text;
        networkModule.changeAddress(port, ip);

        if (networkModule._MyNetworkManager != null)
        {

            if(NetworkClient.active){
                Debug.Log($"client already start via {networkModule._MyNetworkManager.transport}");
                return;
            }

            try
            {
                MyNetworkManager.singleton.StartClient();
            }
            catch (NullReferenceException e)
            {
                Debug.Log(e.Message);
            }


            StartCoroutine(listenClientConnect());
        }
    }

    // as host (server + client)
    public void playLocal()
    {
        string ip = ip_input.text;
        int port = int.Parse(port_input.text);
        networkModule.changeAddress(port);

        if (networkModule._MyNetworkManager != null)
        {

            if (NetworkServer.active && NetworkClient.active)
            {
                Debug.Log($"host already start via {networkModule._MyNetworkManager.transport}");
                return;
            }

            try
            {
                MyNetworkManager.singleton.StartHost();
            }
            catch (SocketException e)
            {
                Debug.Log($"{e.ErrorCode}");
                if(e.ErrorCode==10048){UIModule.showErrorUI("Server Error", "Other program is using this Port, please try other port", 2f);}
                    
                return;
            }

            StartCoroutine(listenHostConnect());
        }
        
    }
    

    public void quit()
    {
        Application.Quit();
    }

    IEnumerator listenClientConnect()
    {
        
        float timeout = 5f;
        UIModule.showLoadingUI("Connecting to server",
            "Connecting to server " + NetworkManager.singleton.networkAddress, 5f);
        while (!NetworkClient.isConnected && timeout >= 0f)
        {
            //Debug.Log($"timeout:{timeout <= 0f}, connected:{NetworkClient.isConnected}");
            timeout -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (!NetworkClient.isConnected&&timeout <= 0f)
        {
            Debug.Log("connection time out");
            UIModule.showErrorUI("Connection Time Out", "please make sure the address is correct", 2f);
        }

        else if (NetworkClient.isConnected)
        {
            
            Debug.Log(
                $"client connected via {networkModule._MyNetworkManager.transport}, ip: {networkModule._MyNetworkManager.networkAddress}");
            yield return new WaitForSeconds(0.5f);
            //TODO: hide UI
            UIModule.getUIById("reminderUI").gameObject.GetComponent<NotificationManager>().CloseNotification();
            closeMenu();
        }


        yield return null;
    }


    IEnumerator listenHostConnect()
    {

        float timeout = 5f;
        UIModule.showLoadingUI("Starting server", "Starting server "+NetworkManager.singleton.networkAddress, 5f);
        while (!(NetworkServer.active && NetworkClient.active) || timeout<=0f)
        {
            timeout -= 0.5f;
            yield return new WaitForSeconds(0.5f);
        }

        if (NetworkServer.active && NetworkClient.active)
        {
            Debug.Log($"Host started via {networkModule._MyNetworkManager.transport}, ip: {networkModule._MyNetworkManager.networkAddress}");
            
            yield return new WaitForSeconds(0.5f);
            //TODO: hide UI
            UIModule.getUIById("reminderUI").gameObject.GetComponent<NotificationManager>().CloseNotification();

            networkModule._MyNetworkManager.ServerChangeScene("Room");
            closeMenu();
        }
        
        
        yield return null;
    }

    private void closeMenu()
    {
        UIModule.DestroyUI("menu");
    }

    private void Awake()
    {
        _inputModule = App.Modules.Get<InputModule>();
    }

    public void showSetting()
    {
        UIModule.showSetting();
    }
}
