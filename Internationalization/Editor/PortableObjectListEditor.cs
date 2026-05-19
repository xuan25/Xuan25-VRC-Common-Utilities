#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Xuan25.Internationalization.Editor
{
    [CustomEditor(typeof(PortableObjectList))]
    public class PortableObjectListEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            var list = (PortableObjectList)target;

            if (GUILayout.Button("Generate Portable Objects"))
            {
                Generate(list);
            }
        }

        private static void Generate(PortableObjectList list)
        {
            if (list == null)
                return;

            Transform parent = list.transform;

            Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Generate Portable Objects");

            if (list.clearExistingChildren)
            {
                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    GameObject child = parent.GetChild(i).gameObject;
                    Undo.DestroyObjectImmediate(child);
                }
            }

            for (int i = 0; i < list.portableObjectFiles.Count; i++)
            {
                TextAsset file = list.portableObjectFiles[i];

                if (file == null)
                {
                    Debug.LogWarning($"PortableObjectList: index {i} is null, skipped.", list);
                    continue;
                }

                GameObject child = new GameObject($"{list.childNamePrefix}{file.name}");
                Undo.RegisterCreatedObjectUndo(child, "Create Portable Object");

                child.transform.SetParent(parent, false);

                PortableObjectHandle handle = child.AddComponent<PortableObjectHandle>();

                handle.portableObjectFile = file;
                handle.localeHeaderKey = list.localeHeaderKey;
                handle.manager = list.manager;

                EditorUtility.SetDirty(child);
            }

            EditorUtility.SetDirty(list);
        }
    }

}

#endif