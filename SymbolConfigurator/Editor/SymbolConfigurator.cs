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
// #if HINT_USHARP_VIDEO
//         AddDefine("USHARP_VIDEO");
// #else
//         RemoveDefine("USHARP_VIDEO");
// #endif
#if HINT_VIZVID
        AddDefine("VIZVID");
#else
        RemoveDefine("VIZVID");
#endif
#if HINT_YAMASTREAM
        AddDefine("YAMASTREAM");
#else
        RemoveDefine("YAMASTREAM");
#endif
#if HINT_YAMASTREAM_V1
        AddDefine("YAMASTREAM_V1");
#else
        RemoveDefine("YAMASTREAM_V1");
#endif
#if HINT_YAMASTREAM_V2
        AddDefine("YAMASTREAM_V2");
#else
        RemoveDefine("YAMASTREAM_V2");
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
