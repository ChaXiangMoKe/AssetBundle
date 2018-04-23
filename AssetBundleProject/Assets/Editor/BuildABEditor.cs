using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using LuaFramework;

public class BuildABEditor
{

    private static string _exportPath = "";
    private static List<AssetBundleBuild> _abbList = new List<AssetBundleBuild>();
    private static List<string> _abbGUIList = new List<string>();
    private static BuildTarget buildTarget;

    public static void ToolsBuildAB(List<BundleConfig> configList, BuildTarget target)
    {
        buildTarget = target;
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("警告", "游戏正在运行，请停止运行再尝试", "确定");
            return;
        }
        if (configList.Count <= 0)
        {
            return;
        }

        // 资源
        BuildDirectoryCheck();

        Begin();

        for (int i = 0; i < configList.Count; i++)
        {
            var bc = configList[i];

            if (bc.ASeparateFile)
            {
                CreateOneABData(bc.BundleName, bc.ResPath);
            }
            else
            {
                CreateAbData(bc.BundleName, bc.Filter, bc.ResPath);
            }
        }
        EndAsset();

        BuileFileIndex();
    }

    private static void Begin()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _exportPath = Path.Combine(Application.dataPath, "StreamingAssets");
        _abbList.Clear();
    }

    // 检查目录
    private static void BuildDirectoryCheck()
    {

        if (Directory.Exists(Util.DataPath))
        {
            Directory.Delete(Util.DataPath, true);
        }
        string streamPath = Application.streamingAssetsPath;
        if (!Directory.Exists(streamPath))
        {
            Directory.CreateDirectory(streamPath);
        }
    }
    //创建单个文件ABData
    private static void CreateOneABData(string packageName, string path)
    {
        string fp = Path.Combine(RGResource.ROOT_PATH, path);
        string guid = AssetDatabase.AssetPathToGUID(fp);
        string filePath = AssetDatabase.GUIDToAssetPath(guid);

        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = packageName;
        abb.assetNames = new string[1];
        abb.assetNames[0] = filePath;
        _abbGUIList.Add(guid);

        _abbList.Add(abb);
    }

    //创建文件夹ABData
    public static void CreateAbData(string packageName, string filterName, params string[] packagePaths)
    {
        string[] pps = new string[packagePaths.Length];
        for (int i = 0; i < packagePaths.Length; i++)
        {
            pps[i] = Path.Combine(RGResource.ROOT_PATH, packagePaths[i]);
            if (!Directory.Exists(pps[i]))
                Debug.LogError("CreateAbData -> Path does not exis!" + pps[i]);
        }

        string[] guids = AssetDatabase.FindAssets(filterName, pps);

        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = packageName;
        abb.assetNames = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(guids[i]);

            //判断文件夹里的文件是否已经打过包了
            if (_abbGUIList.Contains(guids[i]))
            {
                Debug.LogError("Has Add AB Item: " + filePath);
                continue;
            }
            abb.assetNames[i] = filePath;
        }

        _abbList.Add(abb);
    }

    private static void EndAsset()
    {
        if (!Directory.Exists(_exportPath))
            Directory.CreateDirectory(_exportPath);

        AssetBundleManifest abm = BuildPipeline.BuildAssetBundles(_exportPath, _abbList.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, buildTarget);

        if (abm != null)
        {
            string[] abs = abm.GetAllAssetBundles();
            for (int i = 0; i < abs.Length; i++)
            {
                Debug.Log("AB :" + abs[i]);
            }
        }
    }

    #region Build File Index 生成file.txt
    public static void BuileFileIndex()
    {
        string buildVersion = VersionInfo.BundleVersion;

        // file.txt
        List<string> _files = new List<string>();

        string resPath = _exportPath;
        if (!Directory.Exists(resPath))
            Directory.CreateDirectory(resPath);

        string newFilePath = string.Format("{0}{1}", resPath, "/files.txt");
        if (File.Exists(newFilePath))
            File.Delete(newFilePath);

        string[] allFiles = Directory.GetFiles(resPath, "*.*", SearchOption.AllDirectories);
        _files.AddRange(allFiles);

        var fs = new FileStream(newFilePath, FileMode.CreateNew);
        var sw = new StreamWriter(fs);
        for (int i = 0; i < _files.Count; i++)
        {
            string file = _files[i];
            if (file.EndsWith(".DS_Store")) 
                continue;

            if (file.EndsWith("StreamingAssets"))
                continue;

            string ext = Path.GetExtension(file);
            if (ext.Equals(".meta"))
                continue;

            if (ext.Equals(".bat"))
                continue;

            if (ext.Equals(".txt"))
                continue;

            if (ext.Equals(".manifest"))
                continue;

            if (ext.Equals(".mp4"))
                continue;

            if (ext.Equals(".zip"))
                continue;

            var verfile = new VersionFile();
            verfile.Path = file.Replace(resPath + Path.DirectorySeparatorChar, string.Empty).Replace(Path.DirectorySeparatorChar, '/');
            verfile.Hash = Util.md5file(file);
            verfile.Version = buildVersion;
            FileInfo fileInfo = new FileInfo(file);
            verfile.Size = fileInfo.Length;
            sw.WriteLine(verfile);
        }
        sw.Close();
        fs.Close();
    }
    #endregion
}
