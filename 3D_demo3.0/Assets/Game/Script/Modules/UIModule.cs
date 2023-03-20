using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public class UIModule : GameModule
{
    public static GameObject MainCanvas;
    private static Transform _content;

    private static Dictionary<string, GameObject> _displayedUI;

    public override void OnRegister()
    {
        CreateMainCanvas();
        _displayedUI = new Dictionary<string, GameObject>();
    }

    private static void CreateMainCanvas()
    {
        MainCanvas = Instantiate(Resources.Load<GameObject>("Defaults/MainCanvas"));
        MainCanvas.name = "MainCanvas";
        _content = MainCanvas.transform.GetChild(0);
        DontDestroyOnLoad(MainCanvas);
    }

    public static GameObject ShowUI(GameObject go)
    {
        string id = go.name;
        if (_displayedUI.ContainsKey(go.name))
        {
            _displayedUI[go.name].transform.SetAsLastSibling();
            return _displayedUI[go.name];
        }
        go = Instantiate(go, _content, false);
        go.transform.SetAsLastSibling();
        go.name = id;
        _displayedUI.Add(id, go);
        return go;
    }

    public static void DestroyUI(string id)
    {
        if (!_displayedUI.ContainsKey(id))
        {
            Debug.Log($"no {id} exists.");
            return;
        }

        Destroy(_displayedUI[id]);
        _displayedUI.Remove(id);
    }

    public static void DestroyUI(GameObject go)
    {
        DestroyUI(go.name);
    }


    public static void hideUI(GameObject go)
    {
        if (!_displayedUI.ContainsKey(go.name))
        {
            Debug.Log($"no {go.name} exists.");
            return;
        }
        _displayedUI[go.name].SetActive(false);
    }

    
    public static GameObject getUIById(string id)
    {
        return _displayedUI[id];
    }

    public override void OnUnregister()
    {
        //
    }
    #region notification ui
    
    public static GameObject showLoadingUI(string title, string content, float timeout)
    {
        GameObject pref=new GameObject();
        
        if (!_displayedUI.ContainsKey("reminderUI"))
        {
            pref=Resources.Load<GameObject>("Prefabs/UI/notification/Popup Notification");
        }
        
        pref.name = "reminderUI";
        GameObject loading =ShowUI(pref);
        
        loading.gameObject.transform.SetParent(_content.transform,false);

        Sprite loadingImage = Resources.Load<Sprite>("static/128px/UI_Icon_Refresh");

        //  TODO 开始客制化ui
        if (loading != null)
        {
            NotificationManager notifi=loading.gameObject.GetComponent<NotificationManager>();
            UIManagerNotification notifiManager=loading.gameObject.GetComponent<UIManagerNotification>();
            notifi.title = title;
            notifi.description = content;
            notifi.icon = loadingImage;
            notifi.enableTimer = true;
            notifiManager.overrideColors = true;
            notifiManager.background.color =new Color(0/255f,100/255f,255/255,240/255f);
            notifi.timer = timeout;
            notifi.UpdateUI();
            
            notifi.OpenNotification();
        }
        return loading;
    }

    public static GameObject showErrorUI(string title, string content, float timeout)
    {
        GameObject pref=new GameObject();
        
        if (!_displayedUI.ContainsKey("reminderUI"))
        {
            pref=Resources.Load<GameObject>("Prefabs/UI/notification/Popup Notification");
        }
        
        pref.name = "reminderUI";
        GameObject loading =ShowUI(pref);
        
        
        loading.gameObject.transform.SetParent(_content.transform,false);

        Sprite loadingImage = Resources.Load<Sprite>("static/128px/UI_Icon_BtnPsCross");
        
        //  TODO 开始客制化ui
        if (loading != null)
        {
            NotificationManager notifi=loading.gameObject.GetComponent<NotificationManager>();
            UIManagerNotification notifiManager=loading.gameObject.GetComponent<UIManagerNotification>();
            notifi.title = title;
            notifi.description = content;
            notifi.icon = loadingImage;
            notifiManager.overrideColors = true;
            notifiManager.background.color =new Color(160/255f,60/255f,0/255,240/255f);
            notifi.enableTimer = true;
            notifi.timer = timeout;
            notifi.UpdateUI();
            
            notifi.OpenNotification();
        }
        return loading;
    }
    #endregion


    #region base ui

    public static void showMenu()
    {
        GameObject startMenu = Resources.Load<GameObject>("Prefabs/UI/Menu");
        startMenu.name = "menu";
        ShowUI(startMenu);
    }

    public static void showSetting()
    {
        GameObject setting = Resources.Load<GameObject>("Prefabs/UI/setting_page");
        setting.name = "setting";
        UIModule.ShowUI(setting);
    }

    public static void closeMenu()
    {
        UIModule.DestroyUI("setting");
        Globals.Instance._globalEvent.SettingPageOpenState.Invoke(false);
    }

    #endregion
    
}
