#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SceneAssembly
{

    public class SceneAssemblyHelper : IProcessSceneWithReport
    {
        public int callbackOrder => 0;


        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            ScenePart[] sceneParts = Object.FindObjectsOfType<ScenePart>(true);
            if (sceneParts == null || sceneParts.Length == 0)
            {
                Debug.LogWarning($"{GetType()} No SceneAssembly found in scene.");
                return;
            }

            Debug.Log($"{GetType()} Found {sceneParts.Length} SceneAssembly(s) in scene.");

            for (int i = 0; i < sceneParts.Length; i++)
            {
                ScenePart scenePart = sceneParts[i];
                scenePart.transform.localPosition = Vector3.zero;
                scenePart.transform.localRotation = Quaternion.identity;
                scenePart.transform.localScale = Vector3.one;
                scenePart.gameObject.SetActive(true);
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
                Debug.LogError($"{GetType()} No {typeof(T).Name} found in scene.");
                return null;
            }

            return components;
        }
    }

}
#endif
