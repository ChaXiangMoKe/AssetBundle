using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class HotObject
{
    public string package = "";
    public bool selected = false;
}

[System.Serializable]
public class HotScriptableObject:ScriptableObject  {
    public List<HotObject> DataList = new List<HotObject>();

    // 生成全部的资源列表
    public void GenerateAlternativeDataList()
    {
        DataList.Clear();

        //var luas = new string[] { "lua/lua_common.unity3d", "lua/lua_data.unity3d", "lua/lua_logic.unity3d", "lua/lua_ui.unity3d" };
        //for(int i= 0; i < luas.Length; i++)
        //{
        //    var hd = new HotObject();
        //    hd.package = luas[i].ToLower();
        //    hd.selected = false;

        //    DataList.Add(hd);
        //}

        var config = new BundleConfigData();
        foreach (KeyValuePair<int, List<BundleConfig>> itemList in config.ConfigGroupDic)
        {
            for (int i = 0; i < itemList.Value.Count; i++)
            {
                var hd = new HotObject();
                hd.package = itemList.Value[i].BundleName.ToLower();
                hd.selected = false;

                DataList.Add(hd);
            }
        }
    }
    
    public static T CreateHotScriptable<T>()where T : ScriptableObject
    {
        T newScriptable = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(newScriptable, AssetDatabase.GenerateUniqueAssetPath("Assets/Editor/BuildPackage/hot.asset"));
        return newScriptable;
    }

    public static T CreateAlternativeScriptable<T>() where T : ScriptableObject
    {
        T newScriptable = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(newScriptable, AssetDatabase.GenerateUniqueAssetPath("Assets/Editor/BuildPackage/hot_alternative.asset"));
        return newScriptable;
    }
}
