#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OnBuildHookUtility
{

    public class OnBuildHook : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            Process();
        }

        private void Process()
        {
            ProcessRemoveOnBuild();
            ProcessInactivateOnBuild();
#if LTCGI_INCLUDED
            pi.LTCGI.LTCGI_Controller.Singleton.UpdateMaterials(false, null, true);
#endif
        }

        private void ProcessRemoveOnBuild()
        {
            RemoveOnBuild[] removeOnBuilds = FindComponentGlobal<RemoveOnBuild>();
            if (removeOnBuilds == null || removeOnBuilds.Length == 0)
            {
                Debug.LogWarning($"[{GetType()}] No RemoveOnBuild components found in scene.");
                return;
            }
            Debug.Log($"[{GetType()}] Found {removeOnBuilds.Length} RemoveOnBuild components in scene.");
            for (int i = 0; i < removeOnBuilds.Length; i++)
            {
                RemoveOnBuild removeOnBuild = removeOnBuilds[i];
                if (removeOnBuild != null)
                {
                    Object.DestroyImmediate(removeOnBuild.gameObject);
                }
            }
        }

        private void ProcessInactivateOnBuild()
        {
            InactivateOnBuild[] inactivateOnBuilds = FindComponentGlobal<InactivateOnBuild>();
            if (inactivateOnBuilds == null || inactivateOnBuilds.Length == 0)
            {
                Debug.LogWarning($"[{GetType()}] No InactivateOnBuild components found in scene.");
                return;
            }
            Debug.Log($"[{GetType()}] Found {inactivateOnBuilds.Length} InactivateOnBuild components in scene.");
            for (int i = 0; i < inactivateOnBuilds.Length; i++)
            {
                InactivateOnBuild inactivateOnBuild = inactivateOnBuilds[i];
                if (inactivateOnBuild != null)
                {
                    inactivateOnBuild.gameObject.SetActive(false);
                }
            }
        }

        public T FindComponentGlobalFirst<T>() where T : Component
        {
            T[] components = FindComponentGlobal<T>();
            if (components == null || components.Length == 0)
            {
                return null;
            }
            return components[0];
        }

        public T[] FindComponentGlobal<T>() where T : Component
        {
            T[] components = Object.FindObjectsOfType<T>(true);
            return components;
        }
    }

}
#endif