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

        
    }

    private static void LoadAssetAsync(string packageName,string assetName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData = null)
    {
        // todo
    }
    #endregion
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
