using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;



public class Globals : MonoBehaviour
{
    public class GlobalEvent
    {
        public UnityAction<bool> SettingPageOpenState;
    }
    
    public static Globals Instance{ get; private set; }
    public static bool hasRegistered=false;
    public static bool isServer=false;
    [Header("Player Controls")] 
    public GameObject player;
    public ExternalPlayerController externalPlayerController;
    public PlayerHands playerHands;
    public InputModule _InputModule;
    public bool pauseOrNot;
    public GlobalEvent _globalEvent;
    
    //[Header("Game Data")] 
    //[SerializeField] private DataLoader dataLoader;

    //public RuntimeData RuntimeData => dataLoader.RuntimeData;

    //auto start server
    void Start()
    {
        if(isServer){
            networkModule._MyNetworkManager.StartServer();
            networkModule._MyNetworkManager.ServerChangeScene("Main");
        }
        Instance._globalEvent.SettingPageOpenState+=onSettingPageStateChanged;
        Instance._InputModule.BindPerformedAction("Interaction/Menu",onShowSetting);
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            App.Init();
            _InputModule = App.Modules.Get<InputModule>();
            hasRegistered = true;
            Instance = this;
            Instance._globalEvent = new GlobalEvent();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //todo:之后弃用timescale作为游戏暂停，因为服务端暂停会影响客户端运转
    public static void game_Pause()
    {
        Time.timeScale = 0;
        //pauseOrNot = true;
    }
    
    public static void game_Continue()
    {
        Time.timeScale = 1;
    }

    void onShowSetting(InputAction.CallbackContext ctx)
    {
        if (SceneModule.isLoading) return;
        // game_Pause();
        UIModule.showSetting();
        _globalEvent.SettingPageOpenState.Invoke(true);
    }

    void onCloseSetting(InputAction.CallbackContext ctx)
    {
        if (SceneModule.isLoading) return;
        UIModule.closeMenu();
        _globalEvent.SettingPageOpenState.Invoke(false);
    }

    void onSettingPageStateChanged(bool state)
    {
        if (state)
        {
            game_Pause();
            Instance._InputModule.UnBindPerformedAction("Interaction/Menu",onShowSetting);
            Instance._InputModule.BindPerformedAction("Interaction/Menu",onCloseSetting);
        }
        else
        {
            game_Continue();
            Instance._InputModule.UnBindPerformedAction("Interaction/Menu",onCloseSetting);
            Instance._InputModule.BindPerformedAction("Interaction/Menu",onShowSetting);
        }
    }
}
