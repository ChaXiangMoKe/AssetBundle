using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// UPK 文件信息
/// </summary>
public class UPKInfo  {

    // 绝对路径（用于打包UPK使用）
    public string absolutePath;
    // 资源所在相对路径
    public string relativePath;
    // 资源长度
    public long length;

    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="line"></param>
    public void ParesLine(string line)
    {
        var dd = line.Split('|');
        if(dd.Length == 2)
        {
            relativePath = dd[0];
            length = long.Parse(dd[1]);
        }
    }

    public static UPKInfo[] Read(string data)
    {
        var list = new List<UPKInfo>();
        using(var reader = new StringReader(data))
        {
            while(reader.Peek()!= -1){
                var msg = reader.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                {
                    var info = new UPKInfo();
                    info.ParesLine(msg);
                    list.Add(info);
                }
            }
        }
        return list.ToArray();
    }

    public override string ToString()
    {
        return string.Join("|", new[] { relativePath, length.ToString() });
    }
}
