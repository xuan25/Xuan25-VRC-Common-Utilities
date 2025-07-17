#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class DynamicFontAtlasHook : IPreprocessBuildWithReport
{
    public int callbackOrder => 999999;
    public void OnPreprocessBuild(BuildReport report)
    {
        string type = $"t:TMP_FontAsset";
        string[] assets = AssetDatabase.FindAssets(type);
        foreach(string asset in assets)
        {
            string path = AssetDatabase.GUIDToAssetPath(asset);
            TMPro.TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(path);

            if(font.atlasPopulationMode == TMPro.AtlasPopulationMode.Dynamic)
                font.ClearFontAssetData(setAtlasSizeToZero: true);
        }
    }
}
#endif
