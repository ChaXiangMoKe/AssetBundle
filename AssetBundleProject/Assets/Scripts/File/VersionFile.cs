using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaFramework;

public class VersionFile  {

    public string Path { get; set; }

    public string UpkPath
    {
        get
        {
            return Path.Substring(0, Path.Length - 2) + "upk";
        }
    }

    public string Hash { get; set; }

    public string Version { get; set; }

    public long Size { get; set; }

    public string WebPath
    {
        get { return (Version + "/" + Path).Replace("\\", "/"); }
    }
    public string DataLocalPath
    {
        get
        {
            return (Util.DataPath + Path).Replace("//", "/");
        }
    }
    public VersionFile()
    {

    }

    public VersionFile(string data)
    {
        var dd = data.Split('|');
        if(dd.Length == 4)
        {
            Path = dd[0];
            Hash = dd[1];
            Version = dd[2];
            Size = long.Parse(dd[3]);
        }
        else
        {
            Path = dd[0];
            Hash = string.Empty;
            Version = "1";
            Size = 0;
        }
    }

    public static VersionFile[] ReadFileInfo(string data)
    {
        var list = new List<VersionFile>();
        using(var reader = new StringReader(data))
        {
            while(reader.Peek() != -1){
                var msg = reader.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                {
                    list.Add(new VersionFile(msg));
                }
            }
        }
        return list.ToArray();
    }
    public override string ToString()
    {
        return string.Join("|", new[] { Path, Hash, Version, Size.ToString() });
    }
}
