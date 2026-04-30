#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OnBuildHookUtility
{

    public class OnBuildHook : IProcessSceneWithReport
    {
        public int callbackOrder => -1000;

        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            Process();
        }

        private void Process()
        {
            ProcessRemoveOnBuild();
            ProcessInactivateOnBuild();
#if LTCGI_INCLUDED
            if (pi.LTCGI.LTCGI_Controller.Singleton == null)
                return;
            pi.LTCGI.LTCGI_Controller.Singleton.UpdateMaterials(false, null, true);
#endif
        }

        private void ProcessRemoveOnBuild()
        {
            RemoveOnBuild[] removeOnBuilds = FindComponentGlobal<RemoveOnBuild>();
            if (removeOnBuilds == null || removeOnBuilds.Length == 0)
            {
                return;
            }
            for (int i = 0; i < removeOnBuilds.Length; i++)
            {
                RemoveOnBuild removeOnBuild = removeOnBuilds[i];
                if (removeOnBuild == null) continue;

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
                bool remove = removeOnBuild.onDesktop;
#elif UNITY_ANDROID
                bool remove = removeOnBuild.onAndroid;
#elif UNITY_IOS
                bool remove = removeOnBuild.onIOS;
#elif UNITY_VISIONOS
                bool remove = removeOnBuild.onVisionOS;
#else
                bool remove = false;
                Debug.LogWarning($"[{GetType()}] Unsupported platform for RemoveOnBuild component: {Application.platform}. Defaulting to keep.");
#endif

                if (!remove) continue;

                Object.DestroyImmediate(removeOnBuild.gameObject);
            }
        }

        private void ProcessInactivateOnBuild()
        {
            InactivateOnBuild[] inactivateOnBuilds = FindComponentGlobal<InactivateOnBuild>();
            if (inactivateOnBuilds == null || inactivateOnBuilds.Length == 0)
            {
                return;
            }
            for (int i = 0; i < inactivateOnBuilds.Length; i++)
            {
                InactivateOnBuild inactivateOnBuild = inactivateOnBuilds[i];

                if (inactivateOnBuild == null) continue;

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
                bool inactive = inactivateOnBuild.onDesktop;
#elif UNITY_ANDROID
                bool inactive = inactivateOnBuild.onAndroid;
#elif UNITY_IOS
                bool inactive = inactivateOnBuild.onIOS;
#elif UNITY_VISIONOS
                bool inactive = inactivateOnBuild.onVisionOS;
#else
                bool inactive = false;
                Debug.LogWarning($"[{GetType()}] Unsupported platform for InactivateOnBuild component: {Application.platform}. Defaulting to activate.");
#endif

                inactivateOnBuild.gameObject.SetActive(!inactive);
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
                Debug.Log($"[{GetType()}] No {typeof(T).Name} found in scene.");
                return null;
            }

            Debug.Log($"[{GetType()}] Found {components.Length} {typeof(T).Name} in scene.");

            return components;
        }
    }

}
#endif