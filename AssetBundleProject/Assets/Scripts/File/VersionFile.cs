using UnityEngine;
using System.Collections;

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

    public override string ToString()
    {
        return string.Join("|", new[] { Path, Hash, Version, Size.ToString() });
    }
}
