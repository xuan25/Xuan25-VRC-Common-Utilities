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
            ProcessReparentParts();
            ProcessSceneParts();
            ProcessStaticBatchingGroups();
        }

        private void ProcessStaticBatchingGroups()
        {
            StaticBatchingGroup[] staticBatchingGroups = Object.FindObjectsOfType<StaticBatchingGroup>(true);
            if (staticBatchingGroups == null || staticBatchingGroups.Length == 0)
            {
                return;
            }

            for (int i = 0; i < staticBatchingGroups.Length; i++)
            {
                StaticBatchingGroup staticBatchingGroup = staticBatchingGroups[i];
                StaticBatchingUtility.Combine(staticBatchingGroup.gameObject);
                Object.DestroyImmediate(staticBatchingGroup, false);
            }
        }

        private void ProcessSceneParts()
        {
            ScenePart[] sceneParts = Object.FindObjectsOfType<ScenePart>(true);
            if (sceneParts == null || sceneParts.Length == 0)
            {
                return;
            }

            for (int i = 0; i < sceneParts.Length; i++)
            {
                ScenePart scenePart = sceneParts[i];

                scenePart.transform.localPosition = Vector3.zero;
                scenePart.transform.localRotation = Quaternion.identity;
                scenePart.transform.localScale = Vector3.one;
                scenePart.gameObject.SetActive(true);
            }
        }

        private void ProcessReparentParts()
        {
            ReParentPart[] targets = Object.FindObjectsOfType<ReParentPart>(true);
            if (targets == null || targets.Length == 0)
            {
                return;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                ReParentPart target = targets[i];
                string targetParentName = target.targetParentName;
                bool worldPositionStays = target.worldPositionStays;
                Transform targetTransform = target.transform;

                // Remove the marker ReParentPart component

                Object.DestroyImmediate(target, false);

                // Resolve the target parent GameObject

                Transform targetParentTransform;

                if (targetParentName.StartsWith("."))
                {
                    // Relative path
                    string currentPatentFullPath = "";
                    for (Transform t = targetTransform.parent; t != null; t = t.parent)
                    {
                        currentPatentFullPath = "/" + t.name + currentPatentFullPath;
                    }

                    string relativePath = currentPatentFullPath + "/" + targetParentName;

                    string absolutePath = System.IO.Path.GetFullPath(relativePath);

                    // resolve relative path to absolute path
                    targetParentTransform = targetTransform.root.Find(relativePath);
                    if (targetParentTransform == null)
                    {
                        Debug.LogError($"[{nameof(SceneAssemblyHelper)}] Could not find target parent '{targetParentName}' for GameObject '{target.gameObject.name}' using relative path, which resolved to '{absolutePath}'.");
                        continue;
                    }
                }
                else if (targetParentName.StartsWith("/"))
                {
                    // Absolute path
                    targetParentTransform = targetTransform.root.Find(targetParentName);
                    if (targetParentTransform == null)
                    {
                        Debug.LogError($"[{nameof(SceneAssemblyHelper)}] Could not find target parent '{targetParentName}' for GameObject '{target.gameObject.name}' using absolute path.");
                        continue;
                    }
                }
                else
                {
                    // Name only, search globally
                    targetParentTransform = GameObject.Find(targetParentName)?.transform;
                    if (targetParentTransform == null)
                    {
                        Debug.LogError($"[{nameof(SceneAssemblyHelper)}] Could not find target parent '{targetParentName}' for GameObject '{target.gameObject.name}' using name only.");
                        continue;
                    }
                }

                // Re-parent the target GameObject to the found parent
                targetTransform.SetParent(targetParentTransform, worldPositionStays);
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
