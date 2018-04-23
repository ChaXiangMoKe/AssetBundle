using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

public class LoadFromfileExample : MonoBehaviour
{

    // Use this for initialization
    IEnumerator Start()
    {
        string path = @"file:///F:\Test\AssetBundleProject\AssetBundles\cubewall.unity3d";
        //AssetBundle ab = AssetBundle.LoadFromFile("AssetBundles/cubewall.unity3d");
        //GameObject cubePrefab = ab.LoadAsset<GameObject>("CubeWall");
        //Instantiate(cubePrefab);
        //Object[] objs = ab.LoadAllAssets();
        //foreach(Object o in objs)
        //{
        //    Instantiate(o);
        //}

        //第一种加载AB的方式 LoadFromMemoryAsync
        //AssetBundleCreateRequest request =  AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        //yield return request;
        //AssetBundle ab = request.assetBundle;

        //AssetBundle ab = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));

        // 第二种加载AB的方式 LoadFromFile
        //AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
        //yield return request;
        //AssetBundle ab = request.assetBundle;

        // 第三种加载AB的方式 www
        //while (Caching.ready == false)
        //{
        //    yield return null;
        //}

        ////WWW www = WWW.LoadFromCacheOrDownload(path, 1);
        //WWW www = WWW.LoadFromCacheOrDownload(@"http://localhost/AssetBundles/cubewall.unity3d", 1);

        //yield return www;
        //if (string.IsNullOrEmpty(www.error)== false)
        //{
        //    Debug.Log(www.error);
        //    yield break;
        //}
        //AssetBundle ab = www.assetBundle;

        //第四种方式 使用UnityWebRequest

        //string url = @"file:///F:\Test\AssetBundleProject\AssetBundles\cubewall.unity3d";
        string url = @"http://localhost/AssetBundles/cubewall.unity3d";

        UnityWebRequest request = UnityWebRequest.GetAssetBundle(url);
        yield return request.Send();
        //AssetBundle ab = DownloadHandlerAssetBundle.GetContent (request);
        AssetBundle ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;

        // 使用里面的资源
        GameObject cubePrefab = ab.LoadAsset<GameObject>("CubeWall");
        Instantiate(cubePrefab);

        AssetBundle maifestAB = AssetBundle.LoadFromFile("AssetBundles/assetBundles");
        AssetBundleManifest manifest = maifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        //foreach(string name in manifest.GetAllAssetBundles())
        //{
        //    print(name);
        //}
        string[] strs = manifest.GetAllDependencies("cubewall.unity3d");
        foreach (string item in strs)
        {
            print(item);
            AssetBundle.LoadFromFile("AssetBundles/"+ item);
        }


    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AssetBundle ab2 = AssetBundle.LoadFromFile("AssetBundles/share.unity3d");

        }
    }

}
