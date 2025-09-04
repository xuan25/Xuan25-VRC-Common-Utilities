#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DynamicFontAtlasHook : IProcessSceneWithReport
{
    public int callbackOrder => 999999;
    public void OnProcessScene(Scene scene, BuildReport report)
    {
        ClearFontAtlasOnBuild[] clearFontAtlasOnBuilds = FindComponentGlobal<ClearFontAtlasOnBuild>();
        if (clearFontAtlasOnBuilds == null) return;
        
        // check active build target
        bool anyActive = false;

        foreach (ClearFontAtlasOnBuild clearFontAtlas in clearFontAtlasOnBuilds)
        {
            if (clearFontAtlas.gameObject.activeInHierarchy)
            {
                anyActive = true;
                break;
            }
        }

        foreach (ClearFontAtlasOnBuild clearFontAtlas in clearFontAtlasOnBuilds)
        {
            Object.Destroy(clearFontAtlas.gameObject);
        }

        if (!anyActive) return;

        ClearAllDynamicFontAtlas();
    }

    [MenuItem("Tools/Xuan25/Clear All Dynamic TMP Font Atlas")]
    public static void ClearAllDynamicFontAtlas()
    {
        Debug.Log($"[{nameof(DynamicFontAtlasHook)}] Clearing all dynamic TMP font atlas...");
        string type = $"t:TMP_FontAsset";
        string[] assets = AssetDatabase.FindAssets(type);
        foreach (string asset in assets)
        {
            string path = AssetDatabase.GUIDToAssetPath(asset);
            TMPro.TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(path);

            if (font.atlasPopulationMode == TMPro.AtlasPopulationMode.Dynamic)
                font.ClearFontAssetData(setAtlasSizeToZero: true);
        }
    }
    
    public T FindComponentGlobalFirst<T>() where T : Component
    {
        T[] components = FindComponentGlobal<T>();
        if (components == null)
        {
            return null;
        }
        return components[0];
    }

    public T[] FindComponentGlobal<T>() where T : Component
    {
        T[] components = Object.FindObjectsOfType<T>(true);
        if (components.Length == 0)
        {
            Debug.Log($"[{nameof(DynamicFontAtlasHook)}] No {typeof(T).Name} found in scene.");
            return null;
        }

        Debug.Log($"[{nameof(DynamicFontAtlasHook)}] Found {components.Length} {typeof(T).Name} in scene.");

        return components;
    }
}
#endif
