#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

[InitializeOnLoad]
public class SymbolConfigurator : IActiveBuildTargetChanged
{
    public int callbackOrder => 0;

    static void AddDefine(string symbol)
    {
        var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
        string[] defineArray = defines.Split(';');
        if (Array.IndexOf(defineArray, symbol) < 0)
        {
            if (defines.Length > 0)
            {
                defines += ";";
            }
            defines += symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
        }
    }

    static void RemoveDefine(string symbol)
    {
        var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
        string[] defineArray = defines.Split(';');
        defines = string.Join(";", Array.FindAll(defineArray, d => d != symbol));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
    }

    static void AddDefines()
    {
#if USHARP_VIDEO
        AddDefine("USHARP_VIDEO");
#else
        RemoveDefine("USHARP_VIDEO");
#endif
#if VIZVID
        AddDefine("VIZVID");
#else
        RemoveDefine("VIZVID");
#endif
#if YAMASTREAM
        AddDefine("YAMASTREAM");
#else
        RemoveDefine("YAMASTREAM");
#endif
    }

    static SymbolConfigurator()
    {
        AddDefines();
    }

    public void OnActiveBuildTargetChanged(BuildTarget prev, BuildTarget cur)
    {
        AddDefines();
    }
}
#endif
