using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    
    public Toggle screenToggle;
    private bool firstLoad=true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        //TODO:同步ui状态
        screenToggle.isOn = !Screen.fullScreen;
        firstLoad = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void hideUI()
    {
        UIModule.closeMenu();
        // Globals.game_Continue();
    }
    
    public void screenSize() {
        if (!firstLoad)
        {
            if (screenToggle.isOn)
            {
                //Debug.Log("full screen on");
                Resolution[] resolutions = Screen.resolutions;
                Screen.SetResolution(resolutions[resolutions.Length - 1].width,
                    resolutions[resolutions.Length - 1].height, false);
            } //windowed
            else
            {
                //Debug.Log("full screen off");
                Resolution[] resolutions = Screen.resolutions;
                //Debug.Log(resolutions.Length);
                Screen.SetResolution(resolutions[resolutions.Length - 1].width,
                    resolutions[resolutions.Length - 1].height, true);
                //Screen.fullScreen = true;
            } //full screen
        }
    }

    private void OnDisable()
    {
        //todo store the changed data
    }

    public void toMenu()
    {
        if (SceneManager.GetActiveScene().name == "Welcome")
            return;
        
        
        hideUI();
        
        //TODO: disconnect network manager
        try
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                Debug.Log("stop host");
                MyNetworkManager.singleton.StopHost();
            }
            else if (!NetworkServer.active && NetworkClient.isConnected)
            {
                Debug.Log("stop client");
                MyNetworkManager.singleton.StopClient();
            }
            else
            {
                Debug.Log("stop server only");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        
        // load scene
        App.Modules.Get<SceneModule>().asynServerScene("Welcome",false);
    }
}
