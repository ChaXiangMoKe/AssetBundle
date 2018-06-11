using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class HOT_STATE
{
    public static int FAIL = 0;
    public static int SUCCEED = 1;
}

public class HotConfig : MonoBehaviour {
    public int first_upk_download;
    public int first_upk_unpack;
    public int streaming_upk_unpack;

    private string configPath;

    public static HotConfig Create(string path)
    {
        HotConfig hotConfig = new HotConfig();
        hotConfig.first_upk_download = HOT_STATE.FAIL;
        hotConfig.first_upk_unpack = HOT_STATE.FAIL;
        hotConfig.streaming_upk_unpack = HOT_STATE.FAIL;

        hotConfig.configPath = path;
        return hotConfig;
    }

    public static HotConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            return Create(path);
        }
        var obj = JsonUtility.FromJson<HotConfig>(File.ReadAllText(path));
        obj.configPath = path;

        return obj;
    }
    public void Save()
    {
        RGLog.Log("保存 Hot Config 配置 : " + configPath);

        if (File.Exists(configPath))
            File.Delete(configPath);

        StringBuilder hotSb = new StringBuilder();
        hotSb.AppendLine("{");
        hotSb.AppendFormat("\t\"{0}\":{1},\n", "first_upk_download", first_upk_download.ToString());
        hotSb.AppendFormat("\t\"{0}\":{1},\n", "first_upk_unpack", first_upk_unpack.ToString());
        hotSb.AppendFormat("\t\"{0}\":{1},\n", "streaming_upk_unpack", streaming_upk_unpack.ToString());
        hotSb.AppendLine("}");

        File.WriteAllText(configPath, hotSb.ToString());
    }

    // 保存开发模式下开发人员配置
    public void SaveDevelop()
    {
        RGLog.Log("保存 Develop Hot Config 配置 ：" + configPath);

        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }

        StringBuilder hotSb = new StringBuilder();
        hotSb.AppendLine("{");
        hotSb.AppendFormat("\t\"{0}\":{1},\n","first_upk_download",HOT_STATE.SUCCEED);
        hotSb.AppendFormat("\t\"{0}\":{1},\n","first_upk_unpack",HOT_STATE.SUCCEED);
        hotSb.AppendFormat("\t\"{0}\":{1},\n", "streaming_upk_unpack", HOT_STATE.SUCCEED);
        hotSb.AppendLine("}");

        File.WriteAllText(configPath, hotSb.ToString());
    }
}
