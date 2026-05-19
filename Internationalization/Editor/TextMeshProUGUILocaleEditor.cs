#if UNITY_EDITOR

using System.Collections.Generic;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Modes;
using UnityEditor;
using UnityEngine;

namespace Xuan25.Internationalization.Editor
{
    [CustomEditor(typeof(TextMeshProUGUILocale)), CanEditMultipleObjects]
    public class TextMeshProUGUILocaleEditor : UnityEditor.Editor
    {
        private SerializedProperty localeManagerProp;
        private SerializedProperty textComponentProp;
        private SerializedProperty textIdProp;
        private SerializedProperty variablesProp;

        private bool showPreview = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            DrawInfoAndPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInfoAndPreview()
        {
            LocaleManager manager = localeManagerProp.objectReferenceValue as LocaleManager;

            if (string.IsNullOrEmpty(textIdProp.stringValue))
            {
                EditorGUILayout.HelpBox("Text ID is empty. The TextMeshProUGUI text is used as the ID.", MessageType.Info);
            }

            TMPro.TextMeshProUGUI textComponent = textComponentProp.objectReferenceValue as TMPro.TextMeshProUGUI;

            string effectiveId = GetEffectiveTextId(textComponent);

            if (manager == null)
            {
                manager = FindObjectOfType<LocaleManager>();
                if (manager == null)
                {
                    EditorGUILayout.HelpBox("No LocaleManager assigned and found in the scene. Please add one to preview localized text.", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("LocaleManager is not assigned. Auto bind to first LocaleManager found in the scene.", MessageType.Info);
                }
            }

            if (string.IsNullOrEmpty(effectiveId))
            {
                EditorGUILayout.HelpBox("Text ID is empty and no TextMeshProUGUI text was found.", MessageType.Warning);
                return;
            }

            if (textComponent == null)
            {
                textComponent = (target as TextMeshProUGUILocale).GetComponent<TMPro.TextMeshProUGUI>();
                if (textComponent == null)
                {
                    EditorGUILayout.HelpBox("No TextMeshProUGUI component assigned and found on the current object.", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("TextMeshProUGUI component is not assigned. Auto bind to component on current object.", MessageType.Info);
                }
            }

            showPreview = EditorGUILayout.Foldout(showPreview, "Preview", true);
            if (!showPreview)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Manager", GUILayout.Width(EditorGUIUtility.labelWidth - 4));

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(manager, typeof(LocaleManager), true);
                }
            }

            LocaleHandle[] handles = FindLocaleHandles(manager);
            if (handles.Length == 0)
            {
                EditorGUILayout.HelpBox("No LocaleHandle found for the assigned LocaleManager.", MessageType.Info);
                return;
            }

            foreach (LocaleHandle handle in handles)
            {
                try
                {
                    DrawHandlePreview(handle, effectiveId);
                }
                catch (System.Exception ex)
                {
                    EditorGUILayout.HelpBox($"Error drawing preview for handle {handle.name}: {ex.Message}", MessageType.Error);
                }
            }
        }

        private void DrawHandlePreview(LocaleHandle handle, string textId)
        {
            if (handle is PortableObjectHandle portableObjectHandle)
            {
                portableObjectHandle.ForceReload();
            }

            string languageLabel = "(unknown)";
            if (handle.GetLanguageID(out string language))
            {
                languageLabel = language;
            }

            string textValue = textId;
            bool hasText = handle.GetText(textId, out string resolvedText);
            if (hasText)
            {
                textValue = resolvedText;
            }

            string[] variables = new string[variablesProp.arraySize];
            for (int i = 0; i < variables.Length; i++)
            {
                variables[i] = variablesProp.GetArrayElementAtIndex(i).stringValue;
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Handle", GUILayout.Width(EditorGUIUtility.labelWidth - 4));

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField(handle, typeof(LocaleHandle), true);
                    }
                }

                EditorGUILayout.LabelField("Language", languageLabel);

                try
                {
                    textValue = string.Format(textValue, variables);

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextArea(textValue, GUILayout.MinHeight(36));
                    }

                    if (!hasText)
                    {
                        EditorGUILayout.HelpBox("No translation found for this text ID.", MessageType.Warning);
                    }
                }
                catch (System.Exception ex)
                {
                    EditorGUILayout.HelpBox($"Error drawing preview for handle {handle.name}: {ex.Message}", MessageType.Error);
                }
            }
        }

        private string GetEffectiveTextId(TMPro.TextMeshProUGUI textComponent = null)
        {
            if (!string.IsNullOrEmpty(textIdProp.stringValue))
            {
                return textIdProp.stringValue;
            }

            if (textComponent == null)
            {
                textComponent = textComponentProp.objectReferenceValue as TMPro.TextMeshProUGUI;
            }

            if (textComponent != null)
            {
                return textComponent.text;
            }

            return string.Empty;
        }

        private static LocaleHandle[] FindLocaleHandles(LocaleManager manager)
        {
            LocaleHandle[] handles = Object.FindObjectsOfType<LocaleHandle>(true);
            List<LocaleHandle> results = new List<LocaleHandle>();

            foreach (LocaleHandle handle in handles)
            {
                if (handle != null && handle.manager == manager)
                {
                    results.Add(handle);
                }
            }

            return results.ToArray();
        }

        private void OnEnable()
        {
            localeManagerProp = serializedObject.FindProperty("manager");
            textComponentProp = serializedObject.FindProperty("textComponent");
            textIdProp = serializedObject.FindProperty("textID");
            variablesProp = serializedObject.FindProperty("variables");
        }
    }
}

#endif