using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class CreateAssetBundles {

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string dir = "AssetBundles";
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }   

    [MenuItem("Game/Tools")]
    static void BuildBundles()
    {
        string fp = Path.Combine(RGResource.ROOT_PATH, "Prefabs");
        string guid = AssetDatabase.AssetPathToGUID(fp);
        string filePath = AssetDatabase.GUIDToAssetPath(guid);

        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = "Prefabs";
        abb.assetNames = new string[1];
        abb.assetNames[0] = filePath;

        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        list.Add(abb);
        if (!Directory.Exists("AssetBundles"))
            Directory.CreateDirectory("AssetBundles");

        AssetBundleManifest abm = BuildPipeline.BuildAssetBundles("AssetBundles", list.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);

        if (abm != null)
        {
            string[] abs = abm.GetAllAssetBundles();
            for(int i = 0; i < abs.Length; i++)
            {
                Debug.Log("AB:" + abs[i]);
            }
        }
    }
}
