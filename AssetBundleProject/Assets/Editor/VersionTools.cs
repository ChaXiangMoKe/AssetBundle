using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class VersionTools : EditorWindow {

    private static EditorWindow window;

    [MenuItem("Game/打包工具")]
    static void main()
    {
        window = EditorWindow.GetWindow(typeof(VersionTools));
        window.titleContent.text = "打包工具";
        window.Show();
    }

    // 包类型
    private static VersionInfo.BUILD_TYPE bt;
    // 账号类型
    // todo

    // 热更资源
    private static HotScriptableObject hotObject = null;
    private static HotScriptableObject hotAlternativeObject = null;

    GUIStyle fontStyle = new GUIStyle();
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.white;
        GUILayout.Label("正式资源操作", fontStyle);

        // ******************** 此处开始为热更操作 *********************
        GUILayout.Space(20);
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.white;
        GUILayout.Label("热更资源操作",fontStyle);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("清理资源备选名单"))
        {
            RGLog.Debug(" 清理资源备选名单 ");
            GenerateHotAlternativeScriptable();
            ClearHotAlternativeScriptable();
        }
        if (GUILayout.Button("生成资源备选名单"))
        {
            RGLog.Debug(" 生成资源备选名单 ");

            GenerateHotObject();
        }
        GUILayout.EndHorizontal();

        if(hotObject != null)
        {
            if(hotObject.DataList.Count > 0)
            {
                if(GUILayout.Button("build hot bundle"))
                {
                    BuildABTools.BuildHotAll(hotObject.DataList);
                }

                GUILayout.Space(10);

                GUILayout.Label("热更资源名单[" + hotObject.DataList.Count + "]");

                EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(hotObject.DataList.Count * 20));

                for (int i = 0; i < hotObject.DataList.Count; i++)
                {
                    if (hotObject.DataList[i].selected)
                    {
                        EditorGUILayout.LabelField(hotObject.DataList[i].package);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Space(10);
                fontStyle.fontSize = 12;
                fontStyle.normal.textColor = Color.red;
                GUILayout.Label("备注：\n1.请在资源备选名单中选择要热更的资源\n2.然后再执行'生成热更资源名单' ", fontStyle);
            }
        }
        else
        {
            GenerateHotObject();
        }
        EditorGUILayout.EndVertical();
    }
    // 生成热更资源对象
    private static void GenerateHotObject()
    {
        if(hotObject == null)
        {
            hotObject = (HotScriptableObject)AssetDatabase.LoadAssetAtPath(string.Format("Assets{0}Editor{1}BuildPackage{2}hot.asset", Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar),typeof(HotScriptableObject));
            if(hotObject == null)
            {
                var hp = BuildABTools.Replace(string.Format("{0}{1}Editor{2}BuildPackage{3}hot.asset", Application.dataPath, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar));
                if (File.Exists(hp))
                {
                    File.Delete(hp);
                }
                hotObject = HotScriptableObject.CreateHotScriptable<HotScriptableObject>();
            }
        }

        hotObject.DataList.Clear();

        if(hotAlternativeObject != null)
        {
            for (int i = 0; i < hotAlternativeObject.DataList.Count; i++)
            {
                if (hotAlternativeObject.DataList[i].selected)
                {
                    hotObject.DataList.Add(hotAlternativeObject.DataList[i]);
                }
            }
        }
        else
        {
            GenerateHotAlternativeScriptable();
            GenerateHotObject();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    // 生成资源热更备选对象
    private static void GenerateHotAlternativeScriptable()
    {
        if(hotAlternativeObject == null)
        {
            hotAlternativeObject = (HotScriptableObject)AssetDatabase.LoadAssetAtPath(string.Format("Assets{0}Editor{1}BuildPackage{2}hot_alternative.asset", Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar), typeof(HotScriptableObject));
            if(hotAlternativeObject == null)
            {
                var hp = BuildABTools.Replace(string.Format("{0}{1}Editor{2}BuildPackage{3}hot_alternative.asset", Application.dataPath, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar));
                if (File.Exists(hp))
                {
                    File.Delete(hp);
                }
                hotAlternativeObject = HotScriptableObject.CreateAlternativeScriptable<HotScriptableObject>();
                hotAlternativeObject.DataList.Clear();
                hotAlternativeObject.GenerateAlternativeDataList();
            }
        }
    }

    public static void ClearHotAlternativeScriptable()
    {
        hotAlternativeObject.DataList.Clear();
        hotAlternativeObject.GenerateAlternativeDataList();
    }
}
