
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SceneAssembly
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ScenePart : UdonSharpBehaviour
    {
        public bool primary = false;

        public void Start()
        {
            // disable non primary scenes on initialization
            gameObject.SetActive(primary);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ScenePart))]
    public class ScenePartEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ScenePart scenePart = (ScenePart)target;

            // Draw default inspector
            DrawDefaultInspector();

            // check static batching flag
            if (GameObjectUtility.AreStaticEditorFlagsSet(scenePart.gameObject, StaticEditorFlags.BatchingStatic) &&
                scenePart.GetComponent<StaticBatchingGroup>() == null)
            {
                EditorGUILayout.HelpBox("Static Batching is currently enabled for this Scene Part. To minimize potential issues with automatic batching in the editor, it is recommended to use the StaticBatchingGroup component instead, which allows explicit combination of child objects.", MessageType.Warning);
                if (GUILayout.Button("Add StaticBatchingGroup"))
                {
                    Undo.AddComponent<StaticBatchingGroup>(scenePart.gameObject);
                }
            }
        }
    }

#endif

}