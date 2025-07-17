#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RandomHintDisplay.Editor
{
    [CustomEditor(typeof(HintPool))]
    public class HintPoolEditor : UnityEditor.Editor
    {
        // public override void OnInspectorGUI()
        // {
        //     serializedObject.Update();

        //     EditorGUILayout.LabelField("Enter hints here. One hint per line.");

        //     SerializedProperty editorSettingHintTextProperty = serializedObject.FindProperty(nameof(HintPool.editorSettingHintText));

        //     if (editorSettingHintTextProperty.stringValue == null || editorSettingHintTextProperty.stringValue == string.Empty)
        //     {
        //         // init editorSettings.hintText
        //         SerializedProperty hintsProperty = serializedObject.FindProperty(nameof(HintPool.hints));

        //         if (hintsProperty.arraySize != 0)
        //         {
        //             string[] hints = new string[hintsProperty.arraySize];
        //             for (int i = 0; i < hintsProperty.arraySize; i++)
        //             {
        //                 hints[i] = hintsProperty.GetArrayElementAtIndex(i).stringValue;
        //             }
        //             string hintsTextGenerated = string.Join("\n", hints);
        //             editorSettingHintTextProperty.stringValue = hintsTextGenerated;

        //             serializedObject.ApplyModifiedProperties();
        //         }
        //     }

        //     string hintTextUpdated = EditorGUILayout.TextArea(editorSettingHintTextProperty.stringValue);

        //     DrawDefaultInspector();

        //     if (hintTextUpdated != editorSettingHintTextProperty.stringValue)
        //     {
        //         // Update the hints in the HintPool based on the new text area input
        //         editorSettingHintTextProperty.stringValue = hintTextUpdated;
        //         string[] updatedHints = hintTextUpdated.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        //         SerializedProperty hintsProperty = serializedObject.FindProperty(nameof(HintPool.hints));
        //         hintsProperty.arraySize = updatedHints.Length;
        //         for (int i = 0; i < updatedHints.Length; i++)
        //         {
        //             hintsProperty.GetArrayElementAtIndex(i).stringValue = updatedHints[i];
        //         }

        //         serializedObject.ApplyModifiedProperties();
        //     }
        // }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.LabelField("Enter hints here. One hint per line.");

            string hintText;

            // init editorSettings.hintText
            SerializedProperty hintsProperty = serializedObject.FindProperty(nameof(HintPool.hints));

            if (hintsProperty.arraySize != 0)
            {
                string[] hints = new string[hintsProperty.arraySize];
                for (int i = 0; i < hintsProperty.arraySize; i++)
                {
                    hints[i] = hintsProperty.GetArrayElementAtIndex(i).stringValue;
                }
                string hintsTextGenerated = string.Join("\n", hints);
                hintText = hintsTextGenerated;
            }
            else
            {
                hintText = string.Empty;
            }

            string hintTextUpdated = EditorGUILayout.TextArea(hintText);

            if (hintTextUpdated != hintText)
            {
                // Update the hints in the HintPool based on the new text area input
                string[] updatedHints = hintTextUpdated.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

                hintsProperty.arraySize = updatedHints.Length;
                for (int i = 0; i < updatedHints.Length; i++)
                {
                    hintsProperty.GetArrayElementAtIndex(i).stringValue = updatedHints[i];
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

#endif