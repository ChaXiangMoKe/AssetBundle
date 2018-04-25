using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaFramework;

public class PackageManager  {

    // 卸载 package 检查时间
    private const int UNLOAD_TIME = 5;

    // 缓存资源包字典
    private static Dictionary<string, RGPackage> _packageCacheDic = new Dictionary<string, RGPackage>();

    //创建包
    public static RGPackage CreatePackage(string packageName)
    {
        var package = GetPackage(packageName);
        if(package == null)
        {
            package = RGPackage.Create(packageName);
            _packageCacheDic.Add(packageName, package);
        }
        return package;
    }

    // 获得包
    public static RGPackage GetPackage(string packageName)
    {
        RGPackage package = null;
        if(_packageCacheDic.TryGetValue(packageName,out package))
        {
            return package;
        }
        return null;
    }

    // 卸载包
    public static void UnLoadAsset(string resUrl,string suffix)
    {
        string packageName = GetPackageName(resUrl);
        string assetName = GetAssetName(resUrl,suffix);
        var package = GetPackage(packageName);
        if (package != null)
        {
            package.UnloadRes(assetName);
        }
    }

    // 卸载包
    public static void UnloadPackage(string packageName)
    {
        var package = GetPackage(packageName);
        if(package != null)
        {
            package.UnloadAll();
        }
    }
    // 卸载全部包
    public static void UnloadAll()
    {
        using(var i = _packageCacheDic.GetEnumerator())
        {
            RGPackage package = null;
            while (i.MoveNext())
            {
                package = i.Current.Value;
                if (package != null)
                {
                    package.UnloadAll();
                }
            }
        }
        _packageCacheDic.Clear();
    }
    #region 卸载机制
    static float unload_time = 0;
    //固定时间检查bundle资源进行卸载
    public static void UpdateFrame()
    {
        unload_time += Time.deltaTime;
        if (unload_time >= UNLOAD_TIME)
        {
            unload_time = 0;
            //检查需要卸载的包
            CheckUnloadPackage();
        }
    }

    // 自动检查卸载点资源包
    // UI 资源暂时不卸载
    // 已经加载但是还有缓存资源没有加载点不进行卸载
    // 正在加载点资源也不进行卸载
    private static void CheckUnloadPackage()
    {
        List<string> unloadPackageList = new List<string>();
        using(var i = _packageCacheDic.GetEnumerator())
        {
            RGPackage package = null;
            while (i.MoveNext())
            {
                package = i.Current.Value;
                if(package != null)
                {
                    if (package.IsLoadPackage)
                    {
                        if (!package.IsCacheNeedLoad)
                        {
                            if (!package.IsUI && package.IsAutoRelease && !package.IsForverBundle)
                            {
                                RGLog.DebugResLoad("<color=red> Auto Unload bundle</color> -> {0}", package.PackageName);
                                package.UnloadAll(); ;
                                unloadPackageList.Add(package.PackageName);
                            }
                        }
                    }
                }
            }
        }
        for(int i = 0; i < unloadPackageList.Count; i++)
        {
            _packageCacheDic.Remove(unloadPackageList[i]);
        }
    }
    #endregion
    #region tools
    public static string GetPackageName(string path)
    {
        string[] model = path.ToLower().Split(RGResource.PATH_SEPARATOR);

        // 包路径
        string packageUrl = "";
        if (model.Length > 0)
        {
            if (model[0].Equals("effect"))
            {
                //effect
                packageUrl = "effect";
            }else if (model[0].Equals("prefabs"))
            {
                // prefabs
                packageUrl = "prefabs" ;
            }

            RGLog.DebugResLoad("<color=yellow>Package Name = " + packageUrl + "</color>");
            return packageUrl.ToLower();
        }

        RGLog.DebugError("GetPackagePath Error ! Path is Empty");

        return string.Empty;
    }
    /// <summary>
    ///  获得AssetBundle资源包路径
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public static string GetPackagePath(string packageName)
    {
        string p = string.Format("{0}.{1}", packageName, RGResource.PACKAGE_SUFFIX).ToLower();
        return Path.Combine(Util.DataPath, p);
    }

    /// <summary>
    /// 获得包内资源名
    /// </summary>
    /// <param name="resUrl">资源包名称</param>
    /// <param name="suffix">资源后缀名</param>
    /// <returns></returns>
    public static string GetAssetName(string resUrl,string suffix)
    {
        string assetName = Path.Combine(RGResource.ROOT_PATH, string.Format("{0}.{1}", resUrl, suffix));
        return assetName.ToLower();
    }
    #endregion
}
