#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Xuan25.Internationalization.Editor
{
    [CustomEditor(typeof(LocaleManager))]
    public class LocaleManagerEditor : UnityEditor.Editor
    {
        private bool showAssigned = true;
        private bool showUnassigned = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space(8);
            DrawAssignedSection();
            EditorGUILayout.Space(6);
            DrawUnassignedSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAssignedSection()
        {
            showAssigned = EditorGUILayout.Foldout(showAssigned, "Assigned To This Manager", true);
            if (!showAssigned)
            {
                return;
            }

            LocaleManager manager = (LocaleManager)target;
            LocaleHandle[] handles = FindLocaleHandles(manager, true);
            ComponentLocale[] components = FindComponentLocales(manager, true);

            DrawAssignedList("Locale Handles", handles);
            DrawAssignedList("Component Locales", components);
        }

        private void DrawUnassignedSection()
        {
            showUnassigned = EditorGUILayout.Foldout(showUnassigned, "Unassigned", true);
            if (!showUnassigned)
            {
                return;
            }

            LocaleManager manager = (LocaleManager)target;
            LocaleHandle[] handles = FindLocaleHandles(manager, false);
            ComponentLocale[] components = FindComponentLocales(manager, false);

            DrawUnassignedList("Locale Handles", handles, manager);
            DrawUnassignedList("Component Locales", components, manager);
        }

        private static void DrawAssignedList<T>(string label, T[] items) where T : Component
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (items.Length == 0)
            {
                EditorGUILayout.HelpBox("None.", MessageType.Info);
                return;
            }

            foreach (T item in items)
            {
                DrawRow(item, null);
            }
        }

        private static void DrawUnassignedList<T>(string label, T[] items, LocaleManager manager) where T : Component
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (items.Length == 0)
            {
                EditorGUILayout.HelpBox("None.", MessageType.Info);
                return;
            }

            foreach (T item in items)
            {
                DrawRow(item, manager);
            }
        }

        private static void DrawRow(Component item, LocaleManager assignManager)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(item, item.GetType(), true);
                }

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = item.gameObject;
                    EditorGUIUtility.PingObject(item.gameObject);
                }

                if (assignManager != null)
                {
                    if (GUILayout.Button("Add", GUILayout.Width(50)))
                    {
                        AssignManager(item, assignManager);
                    }
                }
            }
        }

        private static void AssignManager(Component item, LocaleManager manager)
        {
            if (item is LocaleHandle localeHandle)
            {
                Undo.RecordObject(localeHandle, "Assign LocaleManager");
                localeHandle.manager = manager;
                EditorUtility.SetDirty(localeHandle);
                return;
            }

            if (item is ComponentLocale componentLocale)
            {
                Undo.RecordObject(componentLocale, "Assign LocaleManager");
                componentLocale.manager = manager;
                EditorUtility.SetDirty(componentLocale);
            }
        }

        private static LocaleHandle[] FindLocaleHandles(LocaleManager manager, bool assignedToManager)
        {
            LocaleHandle[] handles = Object.FindObjectsOfType<LocaleHandle>(true);
            List<LocaleHandle> results = new List<LocaleHandle>();

            foreach (LocaleHandle handle in handles)
            {
                if (handle == null)
                {
                    continue;
                }

                bool isAssigned = handle.manager == manager;
                bool isUnassigned = handle.manager == null;

                if (assignedToManager && isAssigned)
                {
                    results.Add(handle);
                }
                else if (!assignedToManager && isUnassigned)
                {
                    results.Add(handle);
                }
            }

            return results.ToArray();
        }

        private static ComponentLocale[] FindComponentLocales(LocaleManager manager, bool assignedToManager)
        {
            ComponentLocale[] components = Object.FindObjectsOfType<ComponentLocale>(true);
            List<ComponentLocale> results = new List<ComponentLocale>();

            foreach (ComponentLocale component in components)
            {
                if (component == null)
                {
                    continue;
                }

                bool isAssigned = component.manager == manager;
                bool isUnassigned = component.manager == null;

                if (assignedToManager && isAssigned)
                {
                    results.Add(component);
                }
                else if (!assignedToManager && isUnassigned)
                {
                    results.Add(component);
                }
            }

            return results.ToArray();
        }
    }
}

#endif