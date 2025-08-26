#if UNITY_EDITOR
#if LTCGI_INCLUDED
using System.Collections;
using System.Collections.Generic;
using pi.LTCGI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LTCGIBuildTargetUtility
{

    public class LTCGIBuildTargetHook : IActiveBuildTargetChanged, IProcessSceneWithReport
    {
        public int callbackOrder => -1000;

        [MenuItem("Tools/LTCGI/Force Update Build Target")]
        public static void ForceUpdateLTCGIComponents()
        {
            LTCGIBuildTargetHook hook = new LTCGIBuildTargetHook();
            hook.Process(false);
        }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            Process(false);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Process(true);
        }

        public void Process(bool building = false)
        {
            LTCGIBuildTargetConfig[] lTCGIBuildTargetConfigs = FindComponentGlobal<LTCGIBuildTargetConfig>();
            if (lTCGIBuildTargetConfigs == null || lTCGIBuildTargetConfigs.Length == 0)
            {
                return;
            }

            foreach (LTCGIBuildTargetConfig lTCGIBuildTargetConfig in lTCGIBuildTargetConfigs)
            {
                lTCGIBuildTargetConfig.Apply(building);
            }

            Debug.Log($"[{GetType()}] Updated LTCGI components in scene for build target {EditorUserBuildSettings.activeBuildTarget}.");

            LTCGI_Controller.Singleton.UpdateMaterials(false, null, true);
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
                Debug.Log($"[{GetType()}] No {typeof(T).Name} found in scene.");
                return null;
            }

            Debug.Log($"[{GetType()}] Found {components.Length} {typeof(T).Name} in scene.");

            return components;
        }
    }
}
#endif
#endif
