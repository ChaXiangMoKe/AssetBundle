using UnityEngine;
using System.Collections;
using System.IO;
using FairyGUI;
using System;

public class UnpackUPK
{

    private UPKInfo[] infos;

    private string infoFilePath;
    private string upkFilePath;
    private string outPath;
    private string tempUpkPath;
    private string tempOutUpkPath;

    private IUPKUnpack i_unpack;

    private float startTime = 0;

    public UnpackUPK(string _infoFilePath, string _upkFilePath, string _outPath, IUPKUnpack _unpack)
    {
        infoFilePath = _infoFilePath;
        upkFilePath = _upkFilePath;
        outPath = _outPath;
        i_unpack = _unpack;
        tempUpkPath = Path.Combine(outPath, "unpack_temp.upk");
        tempOutUpkPath = Path.Combine(outPath, "unpack_temp_out.upk");

        if (File.Exists(tempUpkPath))
        {
            File.Delete(tempUpkPath);
        }

        if (File.Exists(tempOutUpkPath))
        {
            File.Delete(tempOutUpkPath);
        }

        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
    }

    #region streaming upk
    public void UnpackSteaming()
    {
        Timers.inst.StartCoroutine(UnpackStreamingAsync());
    }

    private IEnumerator UnpackStreamingAsync()
    {
        // 包信息
        if (Application.platform == RuntimePlatform.Android)
        {
            WWW www = new WWW(infoFilePath);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                if (i_unpack != null)
                {
                    i_unpack.OnStreamingError("read streaming upk info www.error = " + www.error);
                }
                yield break;
            }
            else
            {
                infos = UPKInfo.Read(www.text);
            }
        }
        // UPK 文件读取
        if (Application.platform == RuntimePlatform.Android)
        {
            WWW www = new WWW(upkFilePath);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                if (i_unpack != null)
                {
                    i_unpack.OnStreamingError("read streaming upk www.error = " + www.error);
                }
                yield break;
            }
            else
            {
                File.WriteAllBytes(tempUpkPath, www.bytes);
            }
        }
        else
        {
            File.Copy(upkFilePath, tempUpkPath);
        }
        yield return 0;

        // 解压
        if (i_unpack != null)
        {
            i_unpack.OnStreamingDecompression();
        }

        if (VersionInfo.IsCompression)
        {
            bool result = false;
            Timers.inst.StartCoroutine(WaitThreadRun(() =>
            {
                RCompress.DecompressFileLZMA(tempUpkPath, tempOutUpkPath);
                result = true;
            }));

            while (!result)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            tempOutUpkPath = upkFilePath;
        }

        FileStream upkOutStream = new FileStream(tempOutUpkPath, FileMode.Open);

        if(upkOutStream!= null)
        {
            // 解包文件
            long offset = 0;
            for (int i = 0; i < infos.Length; i++)
            {
                var resPath = Path.Combine(outPath, infos[i].relativePath);
                var resLen = infos[i].length;

                var resDir = Path.GetDirectoryName(resPath);
                if (!Directory.Exists(resDir))
                {
                    Directory.CreateDirectory(resDir);
                }

                if (File.Exists(resPath))
                {
                    File.Delete(resPath);
                }

                FileStream outStream = new FileStream(resPath, FileMode.Create);

                var outData = new byte[resLen];
                upkOutStream.Seek(offset, SeekOrigin.Begin);
                upkOutStream.Read(outData, 0, (int)resLen);

                outStream.Write(outData, 0, (int)resLen);
                outStream.Flush();
                outStream.Close();

                offset += resLen;

                if (i_unpack != null)
                {
                    i_unpack.OnStreamingProgress(upkOutStream.Length,offset);
                }
                yield return 0;
            }
            upkOutStream.Close();
        }
        else
        {
            if (i_unpack != null)
            {
                i_unpack.OnStreamingError("read streaming upk stream == null");
            }
        }
        if (i_unpack != null)
        {
            i_unpack.OnStreamingFinished();
        }

        // 删除临时文件
        if (File.Exists(tempUpkPath))
        {
            File.Delete(tempUpkPath);
        }

        if (File.Exists(tempOutUpkPath))
        {
            File.Delete(tempOutUpkPath);
        }
        yield break;
    }
    #endregion

    #region first upk
    public void UnpackFirst()
    {
        Timers.inst.StartCoroutine(UnpackFirstAsync());
    }

    private IEnumerator UnpackFirstAsync()
    {
        startTime = Time.realtimeSinceStartup;

        // 包信息
        if (Application.platform == RuntimePlatform.Android)
        {
            WWW www = new WWW(infoFilePath);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                if (i_unpack != null)
                {
                    i_unpack.OnFirstError("read first upk info www.error= " + www.error);
                }
                yield break;
            }
            else
            {
                infos = UPKInfo.Read(www.text);
            }
        }
        else
        {
            infos = UPKInfo.Read(File.ReadAllText(infoFilePath));
        }

        // 解压
        if(i_unpack != null)
        {
            i_unpack.OnFirstDecompression();
        }

        if (VersionInfo.IsCompression)
        {
            bool result = false;
            Timers.inst.StartCoroutine(WaitThreadRun(()=>
            {
                RCompress.DecompressFileLZMA(tempUpkPath, tempUpkPath);
                result = true;
            }));

            while (!result)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            tempOutUpkPath = upkFilePath;
        }

        FileStream upkOutStream = new FileStream(tempOutUpkPath, FileMode.Open);

        if (upkOutStream != null)
        {
            //解包文件
            long offset = 0;
            for (int i = 0; i < infos.Length; i++)
            {
                var resPath = Path.Combine(outPath, infos[i].relativePath);
                var resLen = infos[i].length;

                var resDir = Path.GetDirectoryName(resPath);
                if (!Directory.Exists(resDir))
                {
                    Directory.CreateDirectory(resDir);
                }

                if (File.Exists(resPath))
                {
                    File.Delete(resPath);
                }

                FileStream outStream = new FileStream(resPath, FileMode.Create);

                var outData = new byte[resLen];
                upkOutStream.Seek(offset, SeekOrigin.Begin);
                upkOutStream.Read(outData, 0, (int)resLen);

                outStream.Write(outData, 0, (int)resLen);
                outStream.Flush();
                outStream.Close();

                offset += resLen;

                if (i_unpack != null)
                {
                    i_unpack.OnStreamingProgress(upkOutStream.Length, offset);
                }
                yield return 0;
            }
            upkOutStream.Close();
        }
        else
        {
            if (i_unpack != null)
            {
                i_unpack.OnStreamingError("read streaming upk stream == null");
            }
        }

        if (i_unpack != null)
        {
            i_unpack.OnStreamingFinished();
        }

        // 删除临时文件
        if (File.Exists(tempUpkPath))
        {
            File.Delete(tempUpkPath);
        }

        if (File.Exists(tempOutUpkPath))
        {
            File.Delete(tempOutUpkPath);
        }
        yield break;
    }
    #endregion

    #region 解压包
    IEnumerator WaitThreadRun(Action action)
    {
        bool waitFinish = false;
        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
            action.Invoke();
            waitFinish = true;
        });
        t.Priority = System.Threading.ThreadPriority.Highest;
        t.Start();

        yield return new WaitUntil(() => waitFinish);
    }
    #endregion
}
