using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class HotDownload : MonoBehaviour {

    // 下载前动作
    private Action downloadBeforeAction;

    // 下载完成动作
    private Action downloadCompleteAction;

    // 下载失败动作
    private Action<string> downloadFailedAction;

    // 正在下载动作
    private Action<float> downloadIngAction;

    // 保存到本地
    private string localPath;

    //url
    private string url;

    private WWW www;

    // 工厂函数
    public static HotDownload Create()
    {
        GameObject go = new GameObject();
        go.name = "HotDownload";
        return go.AddComponent<HotDownload>();
    }

    public void Download(string _url,string _localPath,Action _beforeAction = null,Action _completeAction = null,Action<float> _loadingAction = null,Action<string> _loadFailedAction= null)
    {
        url = _url;
        localPath = _localPath;
        downloadBeforeAction = _beforeAction;
        downloadCompleteAction = _completeAction;
        downloadIngAction = _loadingAction;
        downloadFailedAction = _loadFailedAction;

        StartCoroutine(StartDownload());
    }

    IEnumerator StartDownload()
    {
        if (downloadBeforeAction != null)
        {
            downloadBeforeAction();
        }

        if(downloadIngAction != null)
        {
            downloadIngAction(0);
        }

        www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            if (downloadFailedAction != null)
            {
                downloadFailedAction(www.error);
            }
            www.Dispose();
            www = null;
            yield break;
        }
        else
        {
            if (www.isDone)
            {
                File.WriteAllBytes(localPath, www.bytes);

                www.Dispose();
                www = null;
                
                if(downloadCompleteAction != null)
                {
                    downloadCompleteAction();
                }
            }
        }
    }

    void Update()
    {
        if (www != null)
        {
            if(downloadIngAction != null)
            {
                if (!www.isDone)
                {
                    downloadIngAction(www.progress);
                }
                else
                {
                    downloadIngAction(1);
                }
            }
        }
    }
}
