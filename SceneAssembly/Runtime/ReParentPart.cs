
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SceneAssembly
{
    public class ReParentPart : MonoBehaviour
    {
        [Tooltip("The name of the parent GameObject to reparent this GameObject to.")]
        public string targetParentName = "";

        [Tooltip("Whether to keep the world position, rotation, and scale of the GameObject when reparenting.")]
        public bool worldPositionStays = false;
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ReParentPart))]
    public class ReparentPartEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ReParentPart scenePart = (ReParentPart)target;

            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.HelpBox("This component is used to reparent a GameObject to a specified parent during build time. ", MessageType.Info);
            EditorGUILayout.HelpBox("Parent name can be specified with a Game Object's name, relative path, or absolute path.", MessageType.Info);
            EditorGUILayout.HelpBox("Relative paths begin with a dot (.) and are relative to the current GameObject.", MessageType.Info);
            EditorGUILayout.HelpBox("Absolute paths begin with a slash (/) and are relative to the root of the scene hierarchy.", MessageType.Info);

            // check static batching flag
            // if (GameObjectUtility.AreStaticEditorFlagsSet(scenePart.gameObject, StaticEditorFlags.BatchingStatic) &&
            //     scenePart.GetComponent<StaticBatchingGroup>() == null)
            // {
            //     EditorGUILayout.HelpBox("Static Batching is currently enabled for this Scene Part. To minimize potential issues with automatic batching in the editor, it is recommended to use the StaticBatchingGroup component instead, which allows explicit combination of child objects.", MessageType.Warning);
            //     if (GUILayout.Button("Add StaticBatchingGroup"))
            //     {
            //         Undo.AddComponent<StaticBatchingGroup>(scenePart.gameObject);
            //     }
            // }
        }
    }

#endif

}