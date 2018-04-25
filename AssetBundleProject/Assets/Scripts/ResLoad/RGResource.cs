using UnityEngine;
using System;
using System.Collections;

public class RGResource {

    //采用bundle方式加载
    public static bool USE_BUNDLE_LOAD = true;

    // 路径分割符号
    public static char[] PATH_SEPARATOR = new char[] { '/' };

    //bundle 后缀名称
    public const string PACKAGE_SUFFIX = "ab";

    //资源路径
    public const string ROOT_PATH = "Assets/build";

    // 预加载标识
    public const string PRESTRAIN_FLAG = "PrestrainFlage";

    // 只加载永久bundle，并且不销毁标志
    public const string LOAD_BUNDLE_FOREVER_FLAG = "loadBundleForeverFlag";

    private static T LoadAsset<T>(string resUrl, string suffix) where T : UnityEngine.Object
    {
        string packageName = PackageManager.GetPackageName(resUrl);
        string assetName = PackageManager.GetAssetName(resUrl, suffix);

        var package = PackageManager.CreatePackage(packageName);

        //获取资源，如果已经缓存，就直接返回
        var asset = package.GetAsset(assetName);
        if(asset != null)
        {
            return asset as T;
        }
        else
        {
            asset = package.LoadAsset(assetName);
        }
        if (asset != null)
        {
            return asset as T;
        }
        return null;
    }

    // GameObject
    public static GameObject LoadGameObject(string path)
    {
        return LoadAsset<GameObject>(path, "prefab");
    }

    // Music
    public static AudioClip LoadMusicAsync(string path)
    {
        return LoadAsset<AudioClip>(path, "ogg");
    }

    // SoundEffect
    public static AudioClip LoadSoundEffect(string path)
    {
        return LoadAsset<AudioClip>(path, "ogg");
    }

    // TextAsset
    public static TextAsset LoadByteAsync(string path)
    {
        return LoadAsset<TextAsset>(path, "bytes");
    }

    // TextAsset
    public static TextAsset LoadTxtAsync(string path)
    {
        return LoadAsset<TextAsset>(path, "txt");
    }

    // Texture
    public static Texture LoadTexture(string path)
    {
        return LoadAsset<Texture>(path, "png");
    }

    // Texture2D
    public static Texture2D LoadTexture2D(string path)
    {
        return LoadAsset<Texture2D>(path, "png");
    }

    // UI
    public static TextAsset LoadUIAsset(string path)
    {
        return LoadAsset<TextAsset>(path, "");
    }

    public static TextAsset LoadTextAsset(string path)
    {
        return LoadAsset<TextAsset>(path, "bytes");
    }

    #region load assetbundle
    public static void LoadAsync<T>(string resUrl, string suffix, Action<T, LoadEventData> loadComplete = null, LoadEventData evData = null) where T : UnityEngine.Object
    {
        string packageName = PackageManager.GetPackageName(resUrl);
        string assetName = PackageManager.GetAssetName(resUrl, suffix);
        LoadAssetAsync(packageName, assetName, (obj, ed) =>
        {
            if(loadComplete != null)
            {
                loadComplete.Invoke(obj as T,ed);
            }
        },evData);
    }

    private static void LoadAssetAsync(string packageName,string assetName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData = null)
    {
        var package = PackageManager.CreatePackage(packageName);
        if(evData != null)
        {
            if (evData.data.Length>0)
            {
                if (evData.data[0].ToString().Equals(RGResource.PRESTRAIN_FLAG))
                {
                    package.IsAutoRelease = true;
                }
                else if (evData.data[0].ToString().Equals(RGResource.LOAD_BUNDLE_FOREVER_FLAG))
                {
                    package.IsAutoRelease = false;
                }
            }
        }

        // 场景
        if (package.IsScene)
        {
            package.LoadSceneAsync(loadComplete, evData);
            return;
        }

        // 获取资源，如果已经缓存，就直接返回
        var asset = package.GetAsset(assetName);
        if(asset != null)
        {
            if(loadComplete != null)
            {
                loadComplete(asset, evData);
                RGLog.DebugResLoad("<color=yellow>[ Read Cache Res]</color>" + assetName);
                return;
            }
        }
        RGLog.Log(" 111111111111 " + loadComplete);
        //bundle没有加载到内存，需要先加载bundle
        if (!package.IsLoadPackage)
        {
            package.LoadBundleAsync(assetName, loadComplete, evData);
            RGLog.DebugResLoad("<color=yellow>[ load bundle ]</color>" + assetName);
        }
        else
        {
            //bundle已经加载了，资源还没有加载
            if(package.IsUI || package.IsForverBundle)
            {
                if(loadComplete != null)
                {
                    loadComplete(package.GetBundle(), evData);
                    RGLog.DebugResLoad("<color=yellow>[ Read Cache Res ]</color>"+assetName);
                    return;
                }
            }
            package.LoadAssetAsync(assetName, loadComplete, evData);
            RGLog.DebugResLoad("<color=yellow>[ Load Asset ] </color>" + assetName);
        }
    }
    #endregion

    #region load bundle async
    //Gameobject
    public static void LoadGameObjectAsync(string path,Action<GameObject,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 GameObject Bundle 资源路径为空！！！</color>");
            if(loadComplete != null)
            {
                loadComplete.Invoke(GameObject.CreatePrimitive(PrimitiveType.Cube), ParseFrom(data));
            }
            return;
        }
        LoadAsync<GameObject>(path, "prefab", loadComplete, ParseFrom(data));
    }

    // AutionClip
    public static void LoadMusicAsync(string path,Action<AudioClip,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 Music Bundle 资源路径为空！！！</color>");
            if(loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        LoadAsync<AudioClip>(path, "ogg", loadComplete, ParseFrom(data));
    }

    public static void LoadSoundEffectAsync(string path, Action<AudioClip, LoadEventData> loadComplete, params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 Sound Bundle 资源路径为空！！！</color>");
            if(loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        LoadAsync<AudioClip>(path, "ogg", loadComplete, ParseFrom(data));
    }

    // TextAsset
    public static void LoadByteAsync(string path,Action<TextAsset,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 TextAsset Bundle 资源路径为空！！！</color>");
            if (loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        LoadAsync<TextAsset>(path, "bytes", loadComplete, ParseFrom(data));
    }

    public static void LoadTxtAsync(string path,Action<TextAsset,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 TxtAsset Bundle 资源路径为空！！！</color>");
            if (loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        LoadAsync<TextAsset>(path, "txt", loadComplete, ParseFrom(data));
    }

    // Texture2D
    public static void LoadTexture2DAsync(string path, Action<Texture2D,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 Texture2D Bundle 资源路径为空！！！</color>");
            if (loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        LoadAsync<Texture2D>(path, "png", loadComplete, ParseFrom(data));
    }

    // UI
    public static void LoadUIAsync(string path,Action<AssetBundle,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 UI Bundle 资源路径为空！！！</color>");
            if (loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        LoadAsync<AssetBundle>(path, "", loadComplete, ParseFrom(data));
    }

    // 永久 bundle
    public static void LoadForeverBundleAsync(string path,Action<AssetBundle,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red> 加载 永久 Bundle 资源路径为空！！！</color>");
            if (loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        if(data != null)
        {
            object[] tempData = new object[data.Length + 1];
            tempData.SetValue(LOAD_BUNDLE_FOREVER_FLAG,0);
            for (int i = 0; i < data.Length; i++)
            {
                tempData.SetValue(data[i], i + 1);
            }
            LoadAsync<AssetBundle>(path, "", loadComplete, ParseFrom(tempData));
        }
        else
        {
            object[] tempData = new object[1];
            tempData.SetValue(LOAD_BUNDLE_FOREVER_FLAG,0);
            LoadAsync<AssetBundle>(path, "", loadComplete,ParseFrom(tempData));
        }
    }

    // Scene
    public static void LoadSceneAsync(string path,Action<AssetBundle,LoadEventData> loadComplete,params object[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            RGLog.Error("<color=red>加载 Scene 资源路径为空！！！</color>");
            if(loadComplete != null)
            {
                loadComplete.Invoke(null, ParseFrom(data));
            }
            return;
        }
        LoadAsync<AssetBundle>(path, "unity", loadComplete, ParseFrom(data));
    }
    #endregion

    #region unload
    public static void UnloadAsset(string resUrl,string suffix)
    {
        PackageManager.UnLoadAsset(resUrl, suffix);
    }

    public static void UnloadPackage(string resUrl)
    {
        string packageName = PackageManager.GetPackageName(resUrl);
        PackageManager.UnloadPackage(packageName);
    }

    public static void UnloadAll()
    {
        PackageManager.UnloadAll();
    }
    #endregion
    private static LoadEventData ParseFrom(object[] data)
    {
        //创建事件数据
        var eventData = new LoadEventData();
        eventData.data = data;
        return eventData;
    }
}

// 异步加载事件参数
public class LoadEventData
{
    public object[] data;

    public T Get<T>(int i)
    {
        return (T)data[i];
    }

    public object Get(int i)
    {
        return data[i];
    }
}
