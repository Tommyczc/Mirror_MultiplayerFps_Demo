using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Michsky.UI.ModernUIPack;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AsyncOperation = UnityEngine.AsyncOperation;
using Random = System.Random;

public class SceneModule : GameModule
{
    
    private static List<GameObject> loaderList;
    public static bool isLoading=false;
    
    public override void OnRegister()
    {
        loaderList = new List<GameObject>();
        Init_SceneLoader();
    }

    public override void OnUnregister()
    {
        //throw new System.NotImplementedException();
    }


    public void asynServerScene(string sceneName, bool asyncOperationManager=true)
    {
        if (name == SceneManager.GetActiveScene().name) return;
        StartCoroutine(loadLevelAsync(sceneName,asyncOperationManager));
    }

    private GameObject getRandomTheme()
    {
        return loaderList[new Random().Next(0, loaderList.Count)];
    }

    /**
     * 场景动态加载 (network manager 调用)
     */
    private IEnumerator loadLevelAsync(string name, bool asyncOperationManager)
    {
        
        GameObject loader = getRandomTheme();
        if (loader == null)
            Debug.LogWarning($"cannot find this object in resources {loader.name}");
            yield return null;

        isLoading = true;
        GameObject sceneLoader = UIModule.ShowUI(loader);
        ProgressBar theBar = sceneLoader.gameObject.GetComponentInChildren<ProgressBar>();
        if (theBar == null)
            Debug.LogWarning($"cannot find component Progress bar in the scene loader game object");
            yield return null;
            
            
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        
        
        if (operation == null)
        {
            Debug.LogWarning("cannot find scene operation");
        }
        operation.allowSceneActivation = false;
        if (asyncOperationManager)
        {
            MyNetworkManager.loadingSceneAsync = operation;
        }

        float progress = 0f;
        while (progress<=99f) {
            //Debug.LogWarning(progress);
            //Debug.Log(Mathf.Lerp(0f,1f,operation.progress));
            theBar.ChangeValue(progress);
            progress += 1f;
            yield return new WaitForSeconds(0.01f);
        }
        if (Mathf.Lerp(0f, 1f, operation.progress) >= 0.9f)
        {
            theBar.ChangeValue(100f);
            yield return new WaitForSeconds(0.02f);
            
            Debug.Log("scene is done");
            //yield return new WaitForSeconds(0.3f);
            
            operation.allowSceneActivation = true;
            yield return null;
            
            UIModule.DestroyUI(loader);
            isLoading = false;
        }
    }

    private void Init_SceneLoader()
    {
        GameObject defaultSceneLoader=get_DefaultSceneLoader();
        GameObject theme1=get_SceneLoaderTheme("static/blackhole");
        GameObject theme2=get_SceneLoaderTheme("static/Galaxy");
        
        defaultSceneLoader.transform.SetParent(transform);
        theme1.transform.SetParent(transform);
        theme2.transform.SetParent(transform);

        GameObject[] allLoader = {defaultSceneLoader,theme1,theme2};
        loaderList.AddRange(allLoader);
        
    }

    private static GameObject get_DefaultSceneLoader()
    {
        GameObject sceneLoader=Resources.Load<GameObject>("Prefabs/UI/loading_interface");
        sceneLoader=Instantiate(sceneLoader);
        sceneLoader.name = "DefaultSceneLoader";
        return sceneLoader;
    }

    private static GameObject get_SceneLoaderTheme(string path)
    {
        GameObject sceneLoader=Resources.Load<GameObject>("Prefabs/UI/loading_interface");
        sceneLoader=Instantiate(sceneLoader);
        sceneLoader.name = "SceneLoaderTheme"+loaderList.Count;
        Image image = sceneLoader.gameObject.GetComponent<Image>();
        
        //loading sprite
        Sprite sprite=Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"could not find sprite");
            return sceneLoader;
        }

        
        
        image.sprite = sprite;
        image.color = new Color(205 / 255f, 240 / 255f, 255 / 255, 255 / 255f);
        
        return sceneLoader;
    }
    
}
