using UnityEngine;

public class App 
{
    public static Modules Modules;
    
    /// <summary>
    /// Game Entry Point
    /// 警告: 不知道为什么底下注释的“SubsystemRegistration” 可以在editor启动时运行，但是打包后就不再运行了， 其具体现象是打开 exe文件后只有skybox
    /// 暂时解决方法：global 作为单例（被挂在global物体上）， global调用 App.init()才能运行。
    /// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Init()
    {
        Modules = new Modules();
        
        Modules.Register<InputModule>();
        //Modules.Register<EventModule>();
        Modules.Register<SaveModule>();
        //Modules.Register<BlackboardModule>();
        Modules.Register<UIModule>();
        Modules.Register<networkModule>();
        Modules.Register<SceneModule>();
    }
}
