using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneAssembly
{
    public class StaticBatchingGroup : MonoBehaviour
    {

    }

    [CustomEditor(typeof(StaticBatchingGroup))]
    public class StaticBatchingGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            StaticBatchingGroup staticBatchingGroup = (StaticBatchingGroup)target;

            // Draw default inspector
            DrawDefaultInspector();

            // Check static batching flag
            if (GameObjectUtility.AreStaticEditorFlagsSet(staticBatchingGroup.gameObject, StaticEditorFlags.BatchingStatic))
            {
                EditorGUILayout.HelpBox("Static Batching is currently enabled for this Static Batching Group. It is recommended to disable the Static Batching option to prevent potential conflicts.", MessageType.Warning);

                if (GUILayout.Button("Disable Static Batching"))
                {
                    SetStaticBatchingRecursive(staticBatchingGroup.gameObject, false);
                }
            }
        }

        public static void SetStaticBatchingRecursive(GameObject root, bool enabled)
        {
            if (root == null) return;

            // Gather all GameObjects (root + children, including inactive)
            GameObject[] gos = root.GetComponentsInChildren<Transform>(true)
                        .Select(t => t.gameObject)
                        .ToArray();

            // Start a single undo group
            Undo.IncrementCurrentGroup();
            string opName = (enabled ? "Enable" : "Disable") + " Static Batching (Recursive)";
            Undo.SetCurrentGroupName(opName);
            int group = Undo.GetCurrentGroup();

            // Record all objects in one call (more efficient than per-object)
            Undo.RecordObjects(gos, opName);

            // Apply flag changes
            foreach (GameObject go in gos)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                StaticEditorFlags newFlags = enabled
                    ? (flags | StaticEditorFlags.BatchingStatic)
                    : (flags & ~StaticEditorFlags.BatchingStatic);

                if (newFlags == flags) continue; // skip unchanged

                GameObjectUtility.SetStaticEditorFlags(go, newFlags);
                EditorUtility.SetDirty(go);
            }

            // Collapse all operations into a single undo step
            Undo.CollapseUndoOperations(group);
        }
    }

}