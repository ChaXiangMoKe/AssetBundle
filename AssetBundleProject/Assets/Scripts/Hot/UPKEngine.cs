using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class UPKEngine {

    #region 打包UPK包
    /// <summary>
    /// 打包UPK
    /// </summary>
    /// <param name="infoList"></param>
    /// <param name="upkPath"></param>
    /// <param name="infoPath"></param>
    public static void Pack(List<UPKInfo> infoList,string upkPath,string infoPath)
    {
        //检查upk目录
        var dirUpk = Path.GetDirectoryName(upkPath);
        if (!Directory.Exists(dirUpk))
        {
            Directory.CreateDirectory(dirUpk);
        }
        if (File.Exists(upkPath))
        {
            File.Delete(upkPath);
        }

        //检查 info 目录
        string dirInfo = Path.GetDirectoryName(infoPath);
        if (!Directory.Exists(dirInfo))
        {
            Directory.CreateDirectory(dirInfo);
        }
        if (File.Exists(infoPath))
        {
            File.Delete(infoPath);
        }

        // 打包UPK
        FileStream upkStream = new FileStream(upkPath, FileMode.Create);
        for (int i = 0; i < infoList.Count; i++)
        {
            var info = infoList[i];
            FileStream fileStreamRead = new FileStream(info.absolutePath, FileMode.Open, FileAccess.Read);
            if (fileStreamRead == null)
            {
                RGLog.Log("读取文件失败：" + info.relativePath);
                return;
            }

            byte[] fileData = new byte[info.length];
            fileStreamRead.Read(fileData, 0, (int)info.length);
            fileStreamRead.Close();

            upkStream.Write(fileData, 0, (int)info.length);
        }
    }
    #endregion
}
