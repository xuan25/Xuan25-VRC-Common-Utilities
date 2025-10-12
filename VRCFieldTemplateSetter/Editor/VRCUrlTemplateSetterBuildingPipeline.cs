#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Components;

namespace VRCFieldTemplateSetter
{

    public class VRCUrlTemplateSetterBuildingPipeline : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private void ProcessUrlTemplateSetter(Scene scene)
        {
            VRCUrlTemplateSetter[] urlTemplateSetters = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<VRCUrlTemplateSetter>()).ToArray();

            if (urlTemplateSetters == null || urlTemplateSetters.Length == 0)
            {
                Debug.Log($"[{GetType()}] No {typeof(VRCUrlTemplateSetter).Name} found in scene.");
                return;
            }
            Debug.Log($"[{GetType()}] Found {urlTemplateSetters.Length} {typeof(VRCUrlTemplateSetter).Name} in scene.");

            foreach (VRCUrlTemplateSetter urlTemplateSetter in urlTemplateSetters)
            {
                if (urlTemplateSetter.vRCUrlInputField == null)
                {
                    urlTemplateSetter.vRCUrlInputField = urlTemplateSetter.GetComponent<VRCUrlInputField>();
                }
                UnityEventTools.AddStringPersistentListener(urlTemplateSetter.vRCUrlInputField.onEndEdit, UdonSharpEditorUtility.GetBackingUdonBehaviour(urlTemplateSetter).SendCustomEvent, nameof(urlTemplateSetter.VRCUrlTemplateSetter_OnEndEdit));
                UnityEventTools.AddStringPersistentListener(urlTemplateSetter.vRCUrlInputField.onValueChanged, UdonSharpEditorUtility.GetBackingUdonBehaviour(urlTemplateSetter).SendCustomEvent, nameof(urlTemplateSetter.VRCUrlTemplateSetter_OnValueChanged));
            }
        }

        private void ProcessInputTemplateSetter(Scene scene)
        {
            VRCInputTemplateSetter[] inputTemplateSetters = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<VRCInputTemplateSetter>()).ToArray();

            if (inputTemplateSetters == null || inputTemplateSetters.Length == 0)
            {
                Debug.Log($"[{GetType()}] No {typeof(VRCInputTemplateSetter).Name} found in scene.");
                return;
            }
            Debug.Log($"[{GetType()}] Found {inputTemplateSetters.Length} {typeof(VRCInputTemplateSetter).Name} in scene.");

            foreach (VRCInputTemplateSetter inputTemplateSetter in inputTemplateSetters)
            {
                if (inputTemplateSetter.vRCInputField == null)
                {
                    inputTemplateSetter.vRCInputField = inputTemplateSetter.GetComponent<TMPro.TMP_InputField>();
                }
                UnityEventTools.AddStringPersistentListener(inputTemplateSetter.vRCInputField.onEndEdit, UdonSharpEditorUtility.GetBackingUdonBehaviour(inputTemplateSetter).SendCustomEvent, nameof(inputTemplateSetter.VRCInputTemplateSetter_OnEndEdit));
                UnityEventTools.AddStringPersistentListener(inputTemplateSetter.vRCInputField.onValueChanged, UdonSharpEditorUtility.GetBackingUdonBehaviour(inputTemplateSetter).SendCustomEvent, nameof(inputTemplateSetter.VRCInputTemplateSetter_OnValueChanged));
            }
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log($"[{GetType()}] Processing scene: " + scene.name);
            ProcessUrlTemplateSetter(scene);
            ProcessInputTemplateSetter(scene);
        }
    }

}

#endif