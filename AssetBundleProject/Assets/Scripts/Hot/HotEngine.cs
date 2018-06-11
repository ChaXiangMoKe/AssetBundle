using UnityEngine;
using System.Collections;
using System;
using System.IO;
using LuaFramework;
using System.Collections.Generic;
using FairyGUI;

/*
    热跟新 
*/
public class HotEngine : IUPKUnpack
{
    public enum STATE
    {
        INIT,                   //初始化
        UNPACK_STREAMING_UPK,   //接streaming upk
        DOWNLOAD_FIRST_ZIP,     //下载first upk
        UNPACK_FIRST_UPK,       //解包 first upk
        CHECK_UPDATE,           //检查更新
        DOWNLLOAD_HOT,          //下载热更文件
        ENTER_GAME,             //进入游戏
    }

    private static string CONFIG_PATH
    {
        get
        {
            return Path.Combine(Util.DataPath, "hot_config.json");
        }
    }

    // files.txt 文件路径
    private static string FILES_PATH
    {
        get
        {
            return Path.Combine(Util.DataPath, "files.txt");
        }
    }

    // 配置文件
    private HotConfig hotConfig;

    // server url 
    private string serverUrl;

    // 更新列表
    private List<VersionFile> updateVersionFileList = new List<VersionFile>();

    // 下载总文件数
    private int updateVersionFileCount = 0;

    // 当前下载第几个文件
    private int currentUpdateVersionFileIndex = 0;

    //更新大小
    private long updateSize = 0;

    // 下载器
    private HotDownload download;

    // 状态
    private STATE state;

    // HotEngineUI 接口
    private IHotEngineUI hotUI;

    // 是否开启热更新模式
    public bool OpenHotUpdate = false;

    // 内存大小
    // 解 streaming  upk 需要内存
    private long UnpackStreamingUpkMemorySize = 0;
    private long DownloadFirstMemorySize = 0;
    private long UnpackFirstMemorySize = 0;

    public HotEngine(IHotEngineUI _hotUI)
    {
        hotUI = _hotUI;

        // 内存
        UnpackStreamingUpkMemorySize = 100 * 1048536;
        DownloadFirstMemorySize = 100 * 1048536;
        UnpackFirstMemorySize = 100 * 1048536;
    }

    public void Init()
    {
        state = STATE.INIT;
        hotUI.HOT_InitUI();

        // 服务器上files url
        serverUrl = AppConst.WebUrl;

        // 加载配置
        hotConfig = HotConfig.Load(CONFIG_PATH);

        if (!OpenHotUpdate)
        {
            // 没有开启热更新
            if (CheckUnpackStreaming())
            {
                EnterGame();
            }
            else
            {
                UnpackStreaming();
            }
        }
        else
        {
            // 检查是否需要解压 streaming upk 资源
            if (CheckUnpackStreaming())
            {
                // 进行首包检查
                CheckFirst();
            }
        }
    }

    public void OnFirstDecompression()
    {
        throw new NotImplementedException();
    }

    public void OnFirstError(string error)
    {
        throw new NotImplementedException();
    }

    public void OnFirstFinished()
    {
        throw new NotImplementedException();
    }

    public void OnFirstProgress(float tatal, float current)
    {
        throw new NotImplementedException();
    }

    public void OnStreamingDecompression()
    {
        throw new NotImplementedException();
    }

    public void OnStreamingError(string error)
    {
        throw new NotImplementedException();
    }

    public void OnStreamingFinished()
    {
        throw new NotImplementedException();
    }

    public void OnStreamingProgress(float total, float current)
    {
        throw new NotImplementedException();
    }
    #region streaming 包
    // 解压 streaming upk 包
    public void UnpackStreaming()
    {
        state = STATE.UNPACK_FIRST_UPK;

        hotUI.HOT_InitUnpackStream();

        //清空数据目录
        if (Directory.Exists(Util.DataPath))
        {
            Directory.Delete(Util.DataPath, true);
        }
        Directory.CreateDirectory(Util.DataPath);

        // 设备内存判断
        if (PGameTools.GetStorageSize() < UnpackStreamingUpkMemorySize)
        {
            hotUI.OutOfMemory(UnpackStreamingUpkMemorySize);
            return;
        }
        // 解 streaming upk 包
        string infoFilePath = Path.Combine(Util.AppContentPath(), "streaming.txt");
        string upkFilePath = Path.Combine(Util.AppContentPath(), "streaming.upk");
        UPKEngine.UnpackStreaming(infoFilePath, upkFilePath, Util.DataPath, this);
    }
    /// <summary>
    /// 检查是否需要解压 streaming upk 内的资源
    /// </summary>
    /// <returns></returns>
    public bool CheckUnpackStreaming()
    {
        // 没有创建数据目录 并没有files.txt
        if (!Directory.Exists(Util.DataPath) || !File.Exists(FILES_PATH))
        {
            return false;
        }

        // 检查配置文件内是否已经完成标志完成StreamingAssets资源的释放
        return hotConfig.streaming_upk_unpack == HOT_STATE.SUCCEED;
    }
    #endregion

    #region 首包
    /// <summary>
    /// 检查首包
    /// </summary>
    private void CheckFirst()
    {
        hotUI.HOT_InitFirst();

        if (hotConfig.first_upk_download == HOT_STATE.FAIL)
        {
            // 需要下载首包
            DownloadFirstUpk();
        }
        else if(hotConfig.first_upk_unpack == HOT_STATE.FAIL)
        {
            if (ReadlyUnpackFirstUpk())
            {
                // 需要解首包
                UnpackFirstUpk();
            }
            else
            {
                // 解压 stream upk 资源
                UnpackStreaming();
            }
        }
        else
        {
            // 首包没问题，进行更新检查
            CheckHotUpdate();
        }
    }

    private bool ReadlyUnpackFirstUpk()
    {
        string upkFilePath = Path.Combine(Util.DataPath, "first.upk");
        if (File.Exists(upkFilePath))
        {
            return true;
        }

        // 修改配置
        hotConfig.first_upk_unpack = HOT_STATE.FAIL;
        hotConfig.Save();

        // 下载 first upk
        DownloadFirstUpk();

        return false;
    }
    // 下载首包
    private void DownloadFirstUpk()
    {
        state = STATE.DOWNLOAD_FIRST_ZIP;

        hotUI.HOT_InitDownloadFirst();

        // 内存判断
        if (PGameTools.GetStorageSize() < DownloadFirstMemorySize)
        {
            hotUI.OutOfMemory(DownloadFirstMemorySize);
            return;
        }

        var url = string.Format("{0}{1}/{2}/{3}", serverUrl, GetRouteRoot().TrimStart('/'), VersionInfo.BundleVersion, "first.upk");
        var localPath = Path.Combine(Util.DataPath, "first.upk");
        // 需要下载首包
        if (download == null)
        {
            download = HotDownload.Create();
        }
        download.Download(url, localPath, () =>
        {
            RGLog.Log("首包开始下载");
        },
        () =>
        {
            hotConfig.first_upk_download = HOT_STATE.SUCCEED;
            hotConfig.Save();

            hotUI.HOT_DownloadFirstFinished();

            //解首包 upk
            UnpackFirstUpk();
        },
        (progress)=>
        {
            hotUI.HOT_SetDownloadFirstProgress(progress);
        },
        (error)=>
        {
            hotUI.HOT_DownloadFirstError(error, this);
        });
    }

    // 解首包 upk
    private void UnpackFirstUpk()
    {
        state = STATE.UNPACK_FIRST_UPK;

        hotUI.HOT_InitUnpackFirst();

        // 内存判断
        if (PGameTools.GetStorageSize() < UnpackFirstMemorySize)
        {
            hotUI.HOT_InitUnpackFirst();

            // 内存判断
            if (PGameTools.GetStorageSize() < UnpackFirstMemorySize)
            {
                hotUI.OutOfMemory(UnpackFirstMemorySize);
                return;
            }

            //解 first upk 包
            string infoFilePath = Path.Combine(Util.AppContentPath(), "first.txt");
            string upkFilePath = Path.Combine(Util.DataPath, "first.upk");
            UPKEngine.UnpackFirst(infoFilePath, upkFilePath, Util.DataPath, this);
        }
    }
    #endregion

    #region 热更新
    private void CheckHotUpdate()
    {
        state = STATE.CHECK_UPDATE;

        hotUI.HOT_InitCheckUpdate();

        Timers.inst.StartCoroutine(CheckHotUpdateAsync());
    }

    IEnumerator CheckHotUpdateAsync()
    {
        WWW www = new WWW(FormatUrl(serverUrl, GetRouteRoot() + "files.txt"));
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            hotUI.HOT_CheckHotUpdateError(www.error, this);

            www.Dispose();
            www = null;
        }
        else
        {
            //对比文件是否需要更新
            var serverFileInfos = VersionFile.ReadFileInfo(www.text);

            // 清空更新文件列表
            updateVersionFileList.Clear();
            updateVersionFileCount = 0;
            currentUpdateVersionFileIndex = 0;

            for (int i = 0; i < serverFileInfos.Length; i++)
            {
                var fileInfo = serverFileInfos[i];
                // 本地文件路径
                var localFilePath = fileInfo.DataLocalPath;

                // 文件夹是否存在
                var localFileDir = Path.GetDirectoryName(localFilePath);
                if (!Directory.Exists(localFileDir))
                    Directory.CreateDirectory(localFileDir);

                // 文件服务器地址
                var fileUrl = FormatUrl(serverUrl, GetRouteRoot() + fileInfo.WebPath);

                if (!File.Exists(localFilePath))
                {
                    // 文件不存在，需要更新
                    updateVersionFileList.Add(fileInfo);

                    // 删除本地文件，等待更新
                    updateSize += fileInfo.Size;
                }
                else
                {
                    string serverFileHash = fileInfo.Hash.Trim();
                    string localMd5 = Util.md5file(localFilePath);
                    if (!serverFileHash.Equals(localMd5))
                    {
                        updateVersionFileList.Add(fileInfo);

                        updateSize += fileInfo.Size;

                        // 删除本地文件， 等待更新
                        File.Delete(localFilePath);
                    }
                }
            }
        }

        updateVersionFileCount = updateVersionFileList.Count;
        currentUpdateVersionFileIndex = 0;
        
        if(updateVersionFileCount > 0)
        {
            // 需要更新
            hotUI.HOT_CheckUpdateSure(() => 
            {
                DownloadHot();
            },()=> {
                Application.Quit();
            },
            this);
        }
    }

    private void DownloadHot()
    {
        state = STATE.DOWNLLOAD_HOT;

        hotUI.HOT_InitHot();

        //内存判断
        if (PGameTools.GetStorageSize() < updateSize)
        {
            hotUI.OutOfMemory(updateSize);
            return;
        }

        currentUpdateVersionFileIndex = updateVersionFileCount - updateVersionFileList.Count + 1;

        var fileInfo = updateVersionFileList[0];
        var url = FormatUrl(serverUrl, GetRouteRoot() + fileInfo.WebPath);
        var localPath = fileInfo.DataLocalPath;

        if(download == null)
        {
            download = HotDownload.Create();
        }

        download.Download(url, localPath,
            () =>
            {
                RGLog.Log("热更新下载 url : " + url);
            },
            () =>
            {
                updateVersionFileList.RemoveAt(0);
                if (updateVersionFileList.Count > 0)
                {
                    DownloadHot();
                }
                else
                {
                    hotUI.HOT_HotFinished();
                    EnterGame();
                }
            }, (progress) =>
             {
                 hotUI.HOT_SetHotProgress(updateVersionFileCount, currentUpdateVersionFileIndex, progress);
             }, (error) =>
             {
                 hotUI.HOT_HotError(error, this);
             });
    }
    // 进入游戏
    private void EnterGame()
    {
        state = STATE.ENTER_GAME;
        hotUI.EnterGame();
    }
    #endregion

    #region Retry
    #endregion

    #region 网络检查
    public void Update()
    {
        if (state == STATE.CHECK_UPDATE
            ||state == STATE.DOWNLOAD_FIRST_ZIP
            ||state == STATE.DOWNLLOAD_HOT)
        {
            if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                hotUI.NetworkInterruption(this);
            }
        }
    }
    #endregion

    #region utils
    private string FormatUrl(string webroot,string route)
    {
        string fileUrl = string.Format("{0}{1}", webroot, route.TrimStart('/'));
        return fileUrl;
    }
    private string GetRouteRoot()
    {
        string route = string.Empty;
        // 优先判断是否是内部调试版本
        if (Application.platform == RuntimePlatform.Android)
        {
            route = "android/";
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            route = "ios/";
        }
        else if (Application.isEditor)
        {
#if UNITY_ANDROID
            route = "android/";
#elif UNITY_EDITOR_WIN
            route = "editor_win";
#elif UNITE_EDITOR_OSX
            route = "editor_osx";
#elif UNITY_IPHONE
            rote = "editor_ios/";
#endif
        }
        return route;
    }
#endregion
}
