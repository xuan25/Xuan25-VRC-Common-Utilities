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

public class VRCUrlTemplateSetterBuildingPipeline : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        Debug.Log("VRCUrlTemplateSetterBuildingPipeline Processing scene: " + scene.name);
        VRCUrlTemplateSetter[] urlTemplateSetters = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<VRCUrlTemplateSetter>()).ToArray();

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
}

#endif
