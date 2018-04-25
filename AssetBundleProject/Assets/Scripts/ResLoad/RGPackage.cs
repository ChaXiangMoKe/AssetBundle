using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RGPackage  {

    // 资源包名称
    private string _packageName;
    public string PackageName
    {
        get
        {
            return _packageName;
        }
    }

    // 资源包点相对路径
    private string _packagePath;
    public string PackagePath
    {
        get
        {
            if (string.IsNullOrEmpty(_packagePath))
            {
                _packagePath = PackageManager.GetPackagePath(_packageName);
            }
            return _packagePath;
        }
    }

    // 是否为UI资源包
    public bool IsUI
    {
        get
        {
            return _packageName.StartsWith("ui");
        }
    }

    // 是否为场景资源包
    public bool IsScene
    {
        get
        {
            return _packageName.StartsWith("scenes");
        }
    }

    // 是否自动释放资源
    public bool IsAutoRelease = false;

    // 是否只加载永久bundle
    public bool IsForverBundle = false;

    // 已经加载永久bundle
    public bool IsLoadPackage
    {
        get
        {
            return _bundle != null;
        }
    }

    // 是否已经卸载
    public bool Unloaded { get; private set; }

    // 对应bundle资源
    private AssetBundle _bundle;

    // 加载完成回调函数
    private Dictionary<string, List<Action<UnityEngine.Object, LoadEventData>>> _loadCompleteDict = new Dictionary<string, List<Action<UnityEngine.Object, LoadEventData>>>();
    private Dictionary<string, List<LoadEventData>> _loadEvDataDic = new Dictionary<string, List<LoadEventData>>();

    // 正在加载点时候缓存列表
    private List<string> _cacheAssetNameList = new List<string>();
    private List<Action<UnityEngine.Object, LoadEventData>> _cacheCompleteList = new List<Action<UnityEngine.Object, LoadEventData>>();
    private List<LoadEventData> _cacheEvDataList = new List<LoadEventData>();

    // 是否有缓存资源包等待加载
    public bool IsCacheNeedLoad
    {
        get
        {
            return _cacheAssetNameList.Count > 0;
        }
    }

    // bundle里点资源缓存
    private List<RGRes> _cacheRes = new List<RGRes>();

    // 正在加载资源
    private bool IsLoading = false;

    public RGPackage(string packageName)
    {
        IsLoading = false;
        _packageName = packageName;
        _loadCompleteDict.Clear();
        _loadEvDataDic.Clear();
        _cacheRes.Clear();

        _cacheAssetNameList.Clear();
        _cacheCompleteList.Clear();
        _cacheEvDataList.Clear();
    }

    public static RGPackage Create(string packageName)
    {
        var rg = new RGPackage(packageName);
        return rg;
    }

    public void SetBundle(AssetBundle bundle)
    {
        _bundle = bundle;
    }

    public AssetBundle GetBundle()
    {
        return _bundle;
    }

    public UnityEngine.Object GetAsset(string assetName)
    {
        var res = FindRes(assetName);
        if(res != null)
        {
            int num = res.IncRef();
            return res.GetRes();
        }
        return null;
    }

    public RGRes AddRes(string assetName,UnityEngine.Object obj)
    {
        var res = new RGRes(assetName, obj);
        _cacheRes.Add(res);

        return res;
    }

    private RGRes FindRes(string resName)
    {
        RGRes res = null;
        for (int i = 0; i < _cacheRes.Count; i++)
        {
            res = _cacheRes[i];
            if (res!=null && res.ResName == resName)
            {
                return res;
            }
        }
        return null;
    }

    private bool RemoveRes(RGRes removeRes)
    {
        RGRes res = null;
        for (int i = 0; i < _cacheRes.Count; i++)
        {
            res = _cacheRes[i];
            if (res== removeRes)
            {
                _cacheRes.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    private bool HasRes(string name)
    {
        var res = FindRes(name);
        return res != null;
    }

    #region 同步
    // 同步方式加载bundle
    private AssetBundle LoadBundle(string assetName)
    {
        // 先检查包是否存在
        if (!File.Exists(PackagePath))
        {
            RGLog.Error("<color=red>LoadBundle</color> PackagePath not exits!!!-><color=yellow>{0}</color>", PackagePath);

            return null;
        }
        byte[] stream = null;
        AssetBundle bundle = null;
        stream = File.ReadAllBytes(PackagePath);
        if(stream != null)
        {
            bundle = AssetBundle.LoadFromMemory(stream);
        }
        else
        {
            RGLog.Error("Load Bundle From File Error :" + PackagePath);
        }

        _bundle = bundle;

        IsLoading = false;

        return bundle;
    }

    public UnityEngine.Object LoadAsset(string assetName)
    {
        if(_bundle== null)
        {
            LoadBundle(assetName);
        }

        if(IsUI || IsForverBundle)
        {
            return _bundle;
        }

        RGRes res = FindRes(assetName);
        if(res != null)
        {
            return GetAsset(assetName);
        }

        var asset = _bundle.LoadAsset<UnityEngine.Object>(assetName);
        // 添加资源缓存
        AddRes(assetName, asset);
        return asset;
    }
    #endregion

    #region 异步
    public void LoadBundleAsync(string assetName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        // 先检查包是否存在
        if (!File.Exists(PackagePath))
        {
            RGLog.Error("<color=red>LoadBundleAsync</color> PackagePath not exits!!! -> <color=yellow>{0}</color>", PackagePath);

            if(loadComplete != null)
            {
                loadComplete(null, evData);
            }

            return;
        }

        if (IsLoading)
        {
            AddCache(assetName, loadComplete, evData);

            RGLog.Debug("<color=green>Bundle Loading ! Add Cache {0} -> {1}</color>", _packageName, assetName);
        }
        else
        {
            AddCallback(assetName, loadComplete, evData);

            CoroutineManager.Instance.StartCoroutine(IELoadBundleAsync(assetName, loadComplete, evData));
        }
    }

    public void LoadAssetAsync(string assetName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        // 先查找资源是否加载过，如果加载过资源就直接返回
        RGRes res = FindRes(assetName);
        if (res != null)
        {
            var asset = GetAsset(assetName);
            loadComplete(asset, evData);
            return;
        }

        AddCallback(assetName, loadComplete, evData);

        CoroutineManager.Instance.StartCoroutine(IELoadAssetAsync(assetName, loadComplete, evData));
    }

    private IEnumerator IELoadBundleAsync(string assetName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        bool isLoadCache = false;
        if (!IsLoadPackage)
        {
            var bRequest = AssetBundle.LoadFromFileAsync(PackagePath);

            IsLoading = true;

            yield return bRequest;

            var abRequest = bRequest.assetBundle;

            if(abRequest == null)
            {
                yield break;
            }
            if (!bRequest.isDone)
            {
                yield break;
            }

            _bundle = abRequest;

            IsLoading = false;

            // 加载缓存资源
            isLoadCache = _cacheAssetNameList.Count > 0;
        }

        if (IsUI || IsForverBundle)
        {
            RGLog.DebugResLoad("<color=red>LoadUI</color> {0}", _packageName);
            if(loadComplete != null)
            {
                loadComplete(_bundle, evData);
            }
        }
        else
        {
            CoroutineManager.Instance.StartCoroutine(IELoadAssetAsync(assetName, loadComplete, evData));
            yield return 0;
        }

        // 加载缓存
        CoroutineManager.Instance.StartCoroutine(LoadCache());

        yield return null;
    }

    private IEnumerator IELoadAssetAsync(string assetName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        Debug.Log("name " + assetName);
        var ab = _bundle.LoadAssetAsync<UnityEngine.Object>(assetName);
        yield return ab;

        // 添加资源缓存
        AddRes(assetName, ab.asset);

        if (!_loadCompleteDict.ContainsKey(assetName))
        {
            yield break;
        }

        var callbackList = _loadCompleteDict[assetName];
        var callbackDataList = _loadEvDataDic[assetName];

        for (int i = 0; i < callbackDataList.Count; i++)
        {
            LoadCallback(callbackList[i], GetAsset(assetName), callbackDataList[i]);
            //在加载缓存的时候还存在问题，_loadCompleteDict里面Key不知道怎么会少了一个，如果解决就可以去掉注释
            //yield return 0;
            callbackList[i] = null;
        }

        callbackList = _loadCompleteDict[assetName];
        callbackDataList = _loadEvDataDic[assetName];
        var needDelete = callbackList.TrueForAll(c => c == null);
        if (needDelete)
        {
            _loadCompleteDict.Remove(assetName);
        }
    }
    #endregion

    #region 加载场景
    public void LoadSceneAsync(Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        if(_bundle != null)
        {
            if(loadComplete != null)
            {
                loadComplete(_bundle, evData);
            }
        }
        else
        {
            CoroutineManager.Instance.StartCoroutine(IELoadSceneAsync(loadComplete, evData));
        }
    }

    IEnumerator IELoadSceneAsync(Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        var bRequest = AssetBundle.LoadFromFileAsync(PackagePath);

        IsLoading = true;

        yield return bRequest;
        var abRequest = bRequest.assetBundle;

        if(abRequest== null)
        {
            yield break;
        }
        if (!bRequest.isDone)
        {
            yield break;
        }

        _bundle = abRequest;

        IsLoading = false;

        if(loadComplete != null)
        {
            loadComplete(_bundle, evData);
        }
    }
    #endregion

    public bool UnloadRes(string assetName)
    {
        var res = FindRes(assetName);
        if(res == null)
        {
            RGLog.DebugResLoad("不用重复释放资源：{0}", assetName);
            return false;
        }

        var refCount = res.DecRef();
        {
            RGLog.DebugResLoad("<color=red>Unlod</color> {0}:{1} \tRef:{2}", _packageName, res.ResName, res.RefCount);

            if(refCount <= 0)
            {
                res.UnLoad();
                RemoveRes(res);
                return true;
            }
        }

        return false;
    }

    public void UnloadAll()
    {
        if (_bundle != null)
        {
            RGLog.DebugResLoad("<color=red> unload all bundle </color> -> {0}", _packageName);
            _bundle.Unload(false);
            _bundle = null;
        }

        RGRes res = null;
        for (int i = 0; i < _cacheRes.Count; i++)
        {
            res = _cacheRes[i];
            if (res != null)
            {
                RGLog.DebugResLoad("{0}:{1} \tRef:{2}", _packageName, res.ResName, res.RefCount);

                res.UnLoad();
            }
        }

        _cacheRes.Clear();
        // 清除list所占的内存
        _cacheRes.TrimExcess();

        Unloaded = true;
    }

    public void AddCallback(string assetaName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        if (!_loadCompleteDict.ContainsKey(assetaName))
        {
            var list = new List<Action<UnityEngine.Object, LoadEventData>>();
            _loadCompleteDict.Add(assetaName, list);

            var list2 = new List<LoadEventData>();
            _loadEvDataDic.Add(assetaName, list2);
        }
        _loadCompleteDict[assetaName].Add(loadComplete);
        _loadEvDataDic[assetaName].Add(evData);
    }

    public void LoadCallback(Action<UnityEngine.Object,LoadEventData> callback ,UnityEngine.Object obj,LoadEventData evData)
    {
        if(callback != null)
        {
            callback(obj, evData);
        }
    }

    #region 正在加载bundle 相关处理
    // 正在加载bundle点的时候不能再一次加载，所以需要缓存起床等bundle加载完成之后再加载
    private void AddCache(string assetName,Action<UnityEngine.Object,LoadEventData> loadComplete,LoadEventData evData)
    {
        _cacheAssetNameList.Add(assetName);
        _cacheCompleteList.Add(loadComplete);
        _cacheEvDataList.Add(evData);
    }

    IEnumerator LoadCache()
    {
        string assetName = "";
        Action<UnityEngine.Object, LoadEventData> loadComplete = null;
        LoadEventData evData = null;

        for (int i = 0; i < _cacheAssetNameList.Count; i++)
        {
            assetName = _cacheAssetNameList[i];
            loadComplete = _cacheCompleteList[i];
            evData = _cacheEvDataList[i];

            RGLog.DebugResLoad("Load Cache -> {0}", assetName);

            if(IsUI || IsForverBundle)
            {
                if(loadComplete != null)
                {
                    loadComplete(_bundle, evData);
                }
            }
            else
            {
                LoadAssetAsync(assetName, loadComplete, evData);
            }
            yield return 0;
        }

        _cacheAssetNameList.Clear();
        _cacheCompleteList.Clear();
        _cacheEvDataList.Clear();
    }
    #endregion

}
