using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LuaFramework;
using System.Text;

public class BuildABTools:EditorWindow {
    private static EditorWindow window;

    private static BundleConfigData configData = new BundleConfigData();
    GUIStyle fontStyle = new GUIStyle();
    /// <summary>
    /// 
    /// </summary>
    [MenuItem("Game/BundleTools")]
    static void main()
    {
        window = EditorWindow.GetWindow(typeof(BuildABTools));
        window.titleContent.text = "Bundle Tool";
        window.Show();
    }

    private static Vector2 scrollVec2;
   void OnGUI()
    {
        EditorGUILayout.LabelField("Bundle Version", fontStyle);
        GUILayout.TextField("0.0.1", 25);

        configData = new BundleConfigData();

        var old = GUI.color;
        GUI.color = Color.green;

        if (GUILayout.Button("build ab all", GUILayout.Height(40)))
        {
            BuildAll();
        }

        GUI.color = old;
        scrollVec2 = EditorGUILayout.BeginScrollView(scrollVec2);

        foreach(KeyValuePair<int,List<BundleConfig>> itemList in configData.ConfigGroupDic)
        {
            DrawGroup((Group)itemList.Key);
            for (int i = 0; i < itemList.Value.Count; i++)
            {
                DrawAb(itemList.Value[i]);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void DrawGroup(Group group)
    {
        // 设置背景填充
        fontStyle.normal.background = null;
        //设置字体颜色
        fontStyle.normal.textColor = new Color(1, 0, 0);
        //字体大小
        fontStyle.fontSize = 14;
        fontStyle.fontStyle = FontStyle.Bold;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(group.ToString(), fontStyle);

        var old = GUI.color;
        GUI.color = Color.green;
        if (GUILayout.Button("build" + group.ToString()))
        {
            BuildABEditor.ToolsBuildAB(configData.ConfigGroupDic[(int)group], EditorUserBuildSettings.activeBuildTarget);
            return;
        }
        GUI.color = old;
        EditorGUILayout.EndHorizontal();
    }

    void DrawAb(BundleConfig config)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.TextField(config.BundleName);
        if (GUILayout.Button("Build"))
        {
            List<BundleConfig> l = new List<BundleConfig>();
            l.Add(config);
            BuildABEditor.ToolsBuildAB(l, EditorUserBuildSettings.activeBuildTarget);
            return;
        }
        EditorGUILayout.EndHorizontal();
    }
    public static void BuildAll()
    {
        CleanStreamingAssets();
        BuildABEditor.ToolsBuildAB(configData.ConfigList, EditorUserBuildSettings.activeBuildTarget);
        //构建包
        BuildPackage();
    }

    public static void BuildHotAll(List<HotObject> hotList)
    {
        CleanStreamingAssets();
        BuildABEditor.ToolsBuildAB(configData.ConfigList, EditorUserBuildSettings.activeBuildTarget);
        // 构建包
        BuildPackage();
        // 构建热更
        BuildHotPackage(hotList);
    }
    // 构建包
    public static void BuildPackage()
    {
        if (!File.Exists(Path.Combine(Application.streamingAssetsPath, "files.txt")))
        {
            EditorUtility.DisplayDialog("提示", "请先 build bundle ", "ok");
            return;
        }

        if (VersionInfo.BType != VersionInfo.BUILD_TYPE.DEVELOP)
        {
            // 构建整包资源
            BuildPackageResources();

            // 构建首包资源
            BuildFirstPackageResources();

            // 构建打包资源
            BuildRessources();

            // 生成 streaming upk
            BuildStreamingUPK();

            if (VersionInfo.IsCompression)
            {
                // 压缩 streaming upk
                CompressStreamingUPK();
            }

            // 生成 first upk
            BuildFirstUPK();

            if (VersionInfo.IsCompression)
            {
                // 压缩 first upk
                CompressFirstUPK();
            }

            // 拷贝first upk 到 bundle 目录
            CopyFirstUpkToBundle();
        }
        else
        {
            BuildeDeveloper();
        }


    }

    public static void BuildeDeveloper()
    {
        var filesPath = Path.Combine(Application.streamingAssetsPath, "files.txt");
        var fileData = File.ReadAllText(filesPath);
        var vFiles = ReadFileInfo(fileData);


        var versionPath = GetVersionBundlePath();

        var outPathFix = Util.DataPath;

        if (Directory.Exists(outPathFix))
        {
            Directory.Delete(outPathFix, true);
        }

        //copy
        for(int i = 0; i < vFiles.Length; i++)
        {
            var vfData = vFiles[i];
            var targetPath = Path.Combine(Application.streamingAssetsPath, vfData.Path).Replace("\\","/");
            var outPath = Path.Combine(outPathFix, vfData.Path).Replace("\\", "/");

            var path = Path.GetDirectoryName(outPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.Copy(targetPath, outPath);
        }

        var outFilesPath = Path.Combine(outPathFix, "file.txt").Replace("\\", "/");
        File.Copy(filesPath, outFilesPath);

        // 生成hotCofig
        var hotConfigPath = Path.Combine(Util.DataPath, "hot_config.json");
        HotConfig hot = HotConfig.Create(hotConfigPath);
        hot.SaveDevelop();

        Debug.Log("构建开发人员使用资源完成 ");
    }


    public static void BuildPackageResources()
    {
        var filesPath = Path.Combine(Application.streamingAssetsPath, "files.txt");
        var fileData = File.ReadAllText(filesPath);
        var vFiles = ReadFileInfo(fileData);

        var versionPath = GetVersionBundlePath();

        var outPathFix = Path.Combine(versionPath, "bundle");

        if (Directory.Exists(outPathFix))
        {
            Directory.Delete(outPathFix, true);
        }

        // copy
        for(int i = 0; i < vFiles.Length; i++)
        {
            var vfData = vFiles[i];
            var targetPath = Path.Combine(Application.streamingAssetsPath, vfData.Path).Replace("\\","/");
            var outPath = Path.Combine(outPathFix, "0.0.1/" + vfData.Path).Replace("\\", "/");

            var path = Path.GetDirectoryName(outPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.Copy(targetPath, outPath);
        }

        var outFilesPath = Path.Combine(outPathFix, "files.txt").Replace("\\", "/");
        File.Copy(filesPath, outFilesPath);

        Debug.Log("构建整包资源完成");
    }

    // 构建首包资源
    public static void BuildFirstPackageResources()
    {
        var filesPath = Path.Combine(Application.streamingAssetsPath, "files.txt");
        var fileData = File.ReadAllText(filesPath);
        var vFiles = ReadFileInfo(fileData);

        var versionPath = GetVersionBundlePath();

        var outPathFix = Path.Combine(versionPath, "first");

        if (Directory.Exists(outPathFix))
        {
            Directory.Delete(outPathFix,true);
        }

        Directory.CreateDirectory(outPathFix);

        StringBuilder filesSb = new StringBuilder();

        for (int i = 0; i < vFiles.Length; i++)
        {
            var vfData = vFiles[i];
            var targetPath = Path.Combine(Application.streamingAssetsPath, vfData.Path).Replace("\\", "/");
            var outPath = Path.Combine(outPathFix, vfData.Path).Replace("\\", "/");
            foreach (BundleConfig bc in configData.ConfigList)
            {
                if(bc.NoInPackage && bc.BundleName.ToLower().Equals(vfData.Path.ToLower()))
                {
                    if (File.Exists(targetPath))
                    {
                        var path = Path.GetDirectoryName(outPath);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        filesSb.AppendLine(vfData.ToString());

                        File.Copy(targetPath, outPath);
                    }
                    break;
                }
            }
        }

        var outFilesPath = Path.Combine(outPathFix, "files.txt");
        File.Copy(filesPath, outFilesPath);

        //写入首包文件列表
        var outFirstFilesPath = Path.Combine(outPathFix, "first_files.txt");
        File.WriteAllText(outFirstFilesPath, filesSb.ToString());

        RGLog.Debug("构建首包资源完成");
    }

    public static void BuildRessources()
    {
        //新的包里剩余资源列表
        List<VersionFile> newFiles = new List<VersionFile>();

        //删除不再包里的文件
        var filesPath = Path.Combine(Application.streamingAssetsPath, "files.txt");
        var fileData = File.ReadAllText(filesPath);
        var vFiles = ReadFileInfo(fileData);

        for (int i = 0; i < vFiles.Length; i++)
        {
            var vfData = vFiles[i];
            var targetPath = Path.Combine(Application.streamingAssetsPath, vfData.Path).Replace("\\", "/");
            bool isDelete = false;
            foreach(BundleConfig bc in configData.ConfigList)
            {
               if(bc.NoInPackage && bc.BundleName.ToLower().Equals(vfData.Path.ToLower()))
                {
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                    if (File.Exists(targetPath + ".mainifest"))
                    {
                        File.Delete(targetPath + ".mainifest");
                    }
                    if (File.Exists(targetPath + ".meta"))
                    {
                        File.Delete(targetPath + ".meta");
                    }

                    isDelete = true;
                    break;
                }
            }
            if (!isDelete)
            {
                newFiles.Add(vfData);
            }
        }

        // 删除原来的files.txt
        if (File.Exists(filesPath))
        {
            File.Delete(filesPath);
        }

        var fs = new FileStream(filesPath, FileMode.CreateNew);
        var sw = new StreamWriter(fs);
        for (int i = 0; i < newFiles.Count; i++)
        {
            VersionFile verfile = newFiles[i];
            sw.WriteLine(verfile);
        }
        sw.Close();
        fs.Close();
    }

    public static void CleanStreamingAssets()
    {
        // 资源路径
        string assetPath = Replace(string.Format("{0}{1}{2}", Application.dataPath, Path.DirectorySeparatorChar,"StreamingAssets"));

        // UPK资源路径
        string UPKPath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "streaming.upk"));

        // UPK Info 资源路径
        string InfoPath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "streaming.txt"));

        // file.txt 路径
        string filePath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "files.txt"));

        if (File.Exists(UPKPath))
        {
            File.Delete(UPKPath);
        }
        if (File.Exists(InfoPath))
        {
            File.Delete(InfoPath);
        }
    }

    /// <summary>
    /// 生成 streaming upk包
    /// </summary>
    public static void BuildStreamingUPK()
    {
        //资源路径
        string assetPath = Replace(string.Format("{0}{1}{2}", Application.dataPath, Path.DirectorySeparatorChar, "StreamingAssets"));

        // UPK 资源路径
        string UPKPath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "streaming.upk"));

        // UPK INFO 资源路径
        string InfoPath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "streaming.txt"));

        // files.txt 路径
        string filePath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "files.txt"));

        //开始打包
        List<UPKInfo> infoList = new List<UPKInfo>();

        var fileData = File.ReadAllBytes(filePath);
        var files = ReadFileInfo(filePath);
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fs = Replace(Path.Combine(assetPath, file.Path));

            FileInfo fileInfo = new FileInfo(fs);

            UPKInfo uinfo = new UPKInfo();
            uinfo.relativePath = file.Path;
            uinfo.absolutePath = fs;
            uinfo.length = fileInfo.Length;

            infoList.Add(uinfo);
        }
        // 把files文件也加入
        FileInfo filesInfo = new FileInfo(filePath);
        UPKInfo filesuinfo = new UPKInfo();
        filesuinfo.relativePath = "files.txt";
        filesuinfo.absolutePath = filePath;
        filesuinfo.length = filesInfo.Length;

        infoList.Add(filesuinfo);

        // 打包
        UPKEngine.Pack(infoList, UPKPath, InfoPath);

        // 删除 streamingAssets 内的其他资源
        // 删除文件
        string[] streamingFiles = Directory.GetFiles(assetPath);
        for (int i = 0; i < streamingFiles.Length; i++)
        {
            if (!streamingFiles[i].Equals(UPKPath) && !streamingFiles[i].Equals(InfoPath))
            {
                File.Delete(streamingFiles[i]);
            }
        }
        // 删除文件夹
        string[] streamingDirs = Directory.GetDirectories(assetPath);
        for (int i = 0; i < streamingDirs.Length; i++)
        {
            Directory.Delete(streamingDirs[i], true);
        }

        RGLog.Debug(" 生成 streaming upk 包完成");
     }

    public static void BuildFirstUPK()
    {
        // 版本路径
        string versionPath = GetVersionBundlePath();

        // 资源路径
        string assetPath = Replace(string.Format("{0}{1}{2}",versionPath,Path.DirectorySeparatorChar,"first"));

        // 资源路径
        string firstUpkPath = Replace(string.Format("{0}{1}{2}",versionPath,Path.DirectorySeparatorChar,"first_upk"));

        // upk 资源路径
        string UPKPath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "files.upk"));

        // UPK Info 资源路径
        string streamingAssetPath = Replace(string.Format("{0}{1}{2}", Application.dataPath, Path.DirectorySeparatorChar, "StreamingAssets"));
        string InfoPath = Replace(string.Format("{0}{1}{2}", streamingAssetPath, Path.DirectorySeparatorChar, "first.txt"));

        // fristFile.txt 路径
        string firstFilePath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "first_files.txt"));

        // file.txt 路径
        string filePath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "files.txt"));

        // first upk dir 
        if (Directory.Exists(firstUpkPath))
        {
            Directory.Delete(firstUpkPath, true);
        }
        Directory.CreateDirectory(firstUpkPath);

        // 开始打包
        List<UPKInfo> infoList = new List<UPKInfo>();

        var fileData = File.ReadAllText(firstFilePath);
        var files = ReadFileInfo(fileData);
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fs = Replace(Path.Combine(assetPath, file.Path));

            FileInfo filesInfo = new FileInfo(fs);
            UPKInfo filesuinfo = new UPKInfo();
            filesuinfo.relativePath = "files.txt";
            filesuinfo.absolutePath = filePath;
            filesuinfo.length = filesInfo.Length;

            infoList.Add(filesuinfo);
        }

        // 打包
        UPKEngine.Pack(infoList, UPKPath, InfoPath);

        RGLog.Debug(" 生成 first upk 包");
    }

    // 压缩 streaming upk
    private static void CompressStreamingUPK()
    {
        // 资源路径
        string assetPath = Replace(string.Format("{0}{1}{2}", Application.dataPath, Path.DirectorySeparatorChar, "StreamingAssets"));

        // UPK资源路径
        string UPKPath = Replace(string.Format("{0}{1}{2}", assetPath, Path.DirectorySeparatorChar, "streaming.upk"));

        // UPK资源路径
        string CompressUPKPath = Replace(string.Format("{0}{1}{2}",assetPath,Path.DirectorySeparatorChar,"streaming.zupk"));

        RCompress.CompressFileLZMA(UPKPath, CompressUPKPath);

        //替换之前文件
        if (File.Exists(UPKPath))
        {
            File.Delete(UPKPath);
        }

        File.Copy(CompressUPKPath, UPKPath);

        File.Delete(CompressUPKPath);
    }

    private static void CompressFirstUPK()
    {
        // 版本路径
        string versionPath = GetVersionBundlePath();

        // 资源路径
        string firstUpkPath = Replace(string.Format("{0}{1}{2}", versionPath, Path.DirectorySeparatorChar, "first_upk"));

        //UPK资源路径
        string UPKPath = Replace(string.Format("{0}{1}{2}", firstUpkPath, Path.DirectorySeparatorChar, "first.upk"));

        //UPK资源路径
        string CompressUPKPah = Replace(string.Format("{0}{1}{2}", firstUpkPath, Path.DirectorySeparatorChar, "first.zupk"));

        RCompress.CompressFileLZMA(UPKPath, CompressUPKPah);

        // 替换之前文件 
        if (File.Exists(UPKPath))
        {
            File.Delete(UPKPath);
        }

        File.Copy(CompressUPKPah, UPKPath);

        File.Delete(CompressUPKPah);
    }

    /// <summary>
    /// 拷贝first upk 到 bundle 目录
    /// </summary>
    public static void CopyFirstUpkToBundle()
    {
        // 版本路径
        string versiongPath = GetVersionBundlePath();

        // UPK 资源路径
        string firstUpkPath = Replace(string.Format("{0}{1}{2}{3}{4}", versiongPath, Path.DirectorySeparatorChar, "first_upk", Path.DirectorySeparatorChar, "first.upk"));

        // 目标路径
        string targetPath = Replace(string.Format("{0}{1}{2}{3}{4}{5}{6}",versiongPath,Path.DirectorySeparatorChar,"bundle",Path.DirectorySeparatorChar,VersionInfo.BundleVersion,Path.DirectorySeparatorChar,"first.upk"));

        if (File.Exists(firstUpkPath))
        {
            File.Copy(firstUpkPath, targetPath);
        }

        RGLog.Debug(" first upk copy to bundle finished !");
    }

    public static void BuildHotPackage(List<HotObject> hotList)
    {
        // 把hot 转成字典 用于后面查询
        var hotDic = new Dictionary<string, bool>();
        for (int i = 0; i < hotList.Count; i++)
        {
            hotDic.Add(hotList[i].package, true);
        }

        // 版本路径
        string versionPath = GetVersionBundlePath();

        // hot 路径
        var hotPath = Replace(string.Format("{0}{1}{2}", versionPath, Path.DirectorySeparatorChar, "hot"));
        // hot vesion
        var hotVersionPath = Replace(string.Format("{0}{1}{2}", hotPath, Path.DirectorySeparatorChar, VersionInfo.BundleVersion));
        // hot file
        var hotFilePath = Replace(string.Format("{0}{1}{2}", hotPath, Path.DirectorySeparatorChar, "files.txt"));

        // res 路径
        var resPath = Replace(string.Format("{0}{1}{2}", versionPath, Path.DirectorySeparatorChar, "bundle"));
        // res version
        var resVersionPath = Replace(string.Format("{0}{1}{2}", versionPath, Path.DirectorySeparatorChar, VersionInfo.BundleVersion));
        // res file
        var resFilePath = Replace(string.Format("{0}{1}{2}", versionPath, Path.DirectorySeparatorChar, "files.txt"));

        // 线上file.txt路径
        var liveFilePath = "E:/work2/package/android/file.txt";

        // 清理目录
        ClearDirectory(hotPath);

        // hot new files context dic
        var hotNewFilesContextDic = new Dictionary<string, VersionFile>();
        // hot files context list
        var hotFilesPathContextList = new List<VersionFile>();

        // res files.txt 内容
        var resFiles = ReadFileInfo(File.ReadAllText(resFilePath));
        for (int i = 0; i < resFilePath.Length; i++){
            if (hotDic.ContainsKey(resFiles[i].Path))
            {
                hotNewFilesContextDic.Add(resFiles[i].Path, resFiles[i]);
            }
        }

        // live files.txt 内容
        var liveFiles = ReadFileInfo(File.ReadAllText(liveFilePath));
        for(int i = 0; i < liveFilePath.Length; i++)
        {
            if (hotDic.ContainsKey(resFiles[i].Path))
            {
                hotFilesPathContextList.Add(hotNewFilesContextDic[resFiles[i].Path]);
                RGLog.Debug("hot file -> " + resFiles[i]);
            }
            else
            {
                hotFilesPathContextList.Add(liveFiles[i]);
            }
        }

        // hot files 文件写入
        var hotFs = new FileStream(hotFilePath, FileMode.CreateNew);
        var hotSw = new StreamWriter(hotFs);
        for (int i = 0; i < hotFilesPathContextList.Count; i++)
        {
            VersionFile verfile = hotFilesPathContextList[i];
            hotSw.WriteLine(verfile);
        }
        hotSw.Close();
        hotFs.Close();

        // copy 需要热更的文件到hot目录下
        for(int i = 0; i < hotFilesPathContextList.Count; i++)
        {
            if (hotDic.ContainsKey(hotFilesPathContextList[i].Path))
            {
                var rf = Replace(string.Format("{0}{1}{2}", resVersionPath, Path.DirectorySeparatorChar, hotFilesPathContextList[i].Path));
                var tf = Replace(string.Format("{0}{1}{2}", hotVersionPath, Path.DirectorySeparatorChar, hotFilesPathContextList[i].Path));

                string dir = Path.GetDirectoryName(tf);
                Directory.CreateDirectory(dir);

                File.Copy(rf, tf);
                RGLog.Debug("copy hot file -> " + hotFilesPathContextList[i].Path);
            }
        }

        RGLog.Debug("热更资源构建完毕");
    }

    public static string Replace(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
    }

    public static void ClearDirectory(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
        }
        Directory.CreateDirectory(dirPath);
    }

    // 返回版本资源路径
    public static string GetVersionBundlePath()
    {
        var temp = Application.dataPath.Replace("\\", "/");
        var temp2 = temp.Substring(0, temp.Length - "\\Asset".Length);
        var outPathFix = Path.Combine(temp2, "build/bundle/" + "0.0.1").Replace("\\", "/");
        return outPathFix;
    }

    private static VersionFile[] ReadFileInfo(string data)
    {
        var list = new List<VersionFile>();
        using (var reader = new StringReader(data))
        {
            while (reader.Peek() != -1)
            {
                var msg = reader.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                    list.Add(new VersionFile(msg));
            }
        }
        return list.ToArray();
    }
}

public enum Group
{
    Prefabs,
    Effects,
    Max
}

public class BundleConfig
{
    /// <summary>
    /// bundle 名称
    /// </summary>
    [SerializeField]
    public string BundleName;

    /// <summary>
    /// 资源路径
    /// </summary>
    [SerializeField]
    public string ResPath;

    /// <summary>
    /// 过滤
    /// </summary>
    [SerializeField]
    public string Filter;

    /// <summary>
    /// 是否为单一文件进行打包
    /// </summary>
    [SerializeField]
    public bool ASeparateFile;

    /// <summary>
    /// 分组名称
    /// </summary>
    [SerializeField]
    public Group EGroup;

    /// <summary>
    /// 不打入正式包里
    /// </summary>
    [SerializeField]
    public bool NoInPackage;
}

public class BundleConfigData
{

    public List<BundleConfig> ConfigList = new List<BundleConfig>();

    public Dictionary<int, List<BundleConfig>> ConfigGroupDic = new Dictionary<int, List<BundleConfig>>();
    public BundleConfigData()
    {
        //

        Add(CreateConfig("Prefabs.ab", "t:Prefab", "Prefabs", false, Group.Prefabs,true));
        Add(CreateConfig("Effect.ab", "", "Effect", false, Group.Effects, true));

        //设置分组
        SetGroup();
    }

    BundleConfig CreateConfig(string bn,string filter,string rp,bool asf,Group g,bool notInPackage = false)
    {
        var config = new BundleConfig();
        config.BundleName = bn.Trim();
        config.ResPath = rp.Trim();
        config.Filter = filter;
        config.ASeparateFile = asf;
        config.EGroup = g;
        config.NoInPackage = notInPackage;
        return config;
    }

    void Add(BundleConfig config)
    {
        ConfigList.Add(config);
    }

    void SetGroup()
    {
        int groupCount = (int)Group.Max;

        for (int i = 0; i < groupCount; i++)
        {
            var group = (Group)i;
            List<BundleConfig> cl = new List<BundleConfig>();
            for (int k = 0; k < ConfigList.Count; k++)
            {
                if(group == ConfigList[k].EGroup)
                {
                    cl.Add(ConfigList[k]);
                }
            }
            ConfigGroupDic.Add(i, cl);
        }
    }
}
