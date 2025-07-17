#if UNITY_EDITOR
#if LTCGI_INCLUDED

using System.Collections;
using System.Collections.Generic;
using pi.LTCGI;
using UnityEditor;
using UnityEngine;

namespace LTCGIBuildTargetUtility
{
    public class LTCGIBuildTargetConfig : MonoBehaviour
    {
        // [Header("Behaviour to disable LTCGI components")]
        // public bool inactiveOnTargetChanged = true;
        // public bool removeOnBuild = true;

        [Header("Inactivate LTCGI components on build target change")]
        public bool inactivateOnPC = false;
        public bool inactivateOnAndroid = true;
        public bool inactivateOnIOS = true;
        public bool inactivateOnVisionOS = true;

        [Header("Remove LTCGI components on build")]
        public bool removeOnPC = false;
        public bool removeOnAndroid = true;
        public bool removeOnIOS = true;
        public bool removeOnVisionOS = true;

        public void Apply(bool building = false)
        {
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            UpdateLTCGIComponents(inactivateOnPC, removeOnPC, building);
#elif UNITY_ANDROID
            UpdateLTCGIComponents(inactivateOnAndroid, removeOnAndroid, building);
#elif UNITY_IOS
            UpdateLTCGIComponents(inactivateOnIOS, removeOnIOS, building);
#elif UNITY_VISIONOS
            UpdateLTCGIComponents(inactivateOnVisionOS, removeOnVisionOS, building);
#else
            UpdateLTCGIComponents(true, true, building);
            Debug.LogWarning($"[{GetType()}] Unsupported platform for LTCGI components: {Application.platform}. Defaulting to disable.");
#endif
        }

        public void UpdateLTCGIComponents(bool inactiveOnTargetChanged, bool removeOnBuild, bool building = false)
        {
            if (building)
            {
                if (removeOnBuild)
                {
                    RemoveOnBuild();
                }
            }
            else
            {
                SetActive(!inactiveOnTargetChanged);
            }
        }

        private void RemoveOnBuild()
        {
            LTCGI_Screen lTCGI_Screen = GetComponent<LTCGI_Screen>();
            if (lTCGI_Screen != null)
            {
                Object.DestroyImmediate(lTCGI_Screen);
                Debug.Log($"[{GetType()}] Removing LTCGI_Screen component from {gameObject.name}.");
            }

            LTCGI_Emitter lTCGI_Emitter = GetComponent<LTCGI_Emitter>();
            if (lTCGI_Emitter != null)
            {
                Object.DestroyImmediate(lTCGI_Emitter);
                Debug.Log($"[{GetType()}] Removing LTCGI_Emitter component from {gameObject.name}.");
            }
        }

        private void SetActive(bool active)
        {
            LTCGI_Screen lTCGI_Screen = GetComponent<LTCGI_Screen>();
            if (lTCGI_Screen != null && lTCGI_Screen.enabled != active)
            {
                lTCGI_Screen.enabled = active;
                Debug.Log($"[{GetType()}] Setting LTCGI_Screen component on {gameObject.name} to {active}.");
            }

            LTCGI_Emitter lTCGI_Emitter = GetComponent<LTCGI_Emitter>();
            if (lTCGI_Emitter != null && lTCGI_Emitter.enabled != active)
            {
                lTCGI_Emitter.enabled = active;
                Debug.Log($"[{GetType()}] Setting LTCGI_Emitter component on {gameObject.name} to {active}.");
            }
        }
    }



    [CustomEditor(typeof(LTCGIBuildTargetConfig))]
    [CanEditMultipleObjects]
    public class LTCGIMobileDisableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("LTCGI components attached to this GameObject will be inactivated on build target change and/or removed on build", EditorStyles.boldLabel);
            DrawDefaultInspector();
        }
    }

}

#endif
#endif