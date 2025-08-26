#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnimatorUtilities {

    public class AnimatorUtilitiesBuildingPipeline : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private ContinuousAnimatorDriverSliderController[] GetSliderControllers() => FindComponentGlobal<ContinuousAnimatorDriverSliderController>();
        private DiscreteAnimatorDriverIndexerButtonController[] GetButtonControllers() => FindComponentGlobal<DiscreteAnimatorDriverIndexerButtonController>();

        private void BindSliderEventsToUdonBehaviours<T>(T target, Slider slider, string methodName) where T : UdonSharpBehaviour
        {
            if (slider == null)
            {
                Debug.LogError($"[{nameof(AnimatorUtilitiesBuildingPipeline)}] {target.name} has a null {nameof(Slider)}. Please ensure all sliders are assigned.");
                return;
            }
            UnityEventTools.AddStringPersistentListener(slider.onValueChanged, UdonSharpEditorUtility.GetBackingUdonBehaviour(target).SendCustomEvent, methodName);
        }

        private void BindButtonEventsToUdonBehaviours<T>(T target, Button button, string methodName) where T : UdonSharpBehaviour
        {
            if (button == null)
            {
                Debug.LogError($"[{nameof(AnimatorUtilitiesBuildingPipeline)}] {target.name} has a null {nameof(Button)}. Please ensure all buttons are assigned.");
                return;
            }
            UnityEventTools.AddStringPersistentListener(button.onClick, UdonSharpEditorUtility.GetBackingUdonBehaviour(target).SendCustomEvent, methodName);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log($"[{nameof(AnimatorUtilitiesBuildingPipeline)}] Processing scene: " + scene.name);

            ContinuousAnimatorDriverSliderController[] targets = GetSliderControllers();
            if (targets == null) return;

            foreach (ContinuousAnimatorDriverSliderController target in targets)
            {
                foreach (Slider slider in target.sliders)
                    BindSliderEventsToUdonBehaviours(target, slider, nameof(ContinuousAnimatorDriverSliderController.OnSliderValueChanged));
            }
            
            DiscreteAnimatorDriverIndexerButtonController[] buttonTargets = GetButtonControllers();
            if (buttonTargets == null) return;

            foreach (DiscreteAnimatorDriverIndexerButtonController target in buttonTargets)
            {
                foreach (Button button in target.buttons)
                    BindButtonEventsToUdonBehaviours(target, button, nameof(DiscreteAnimatorDriverIndexerButtonController.OnButtonClicked));
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
                Debug.LogWarning($"[{nameof(AnimatorUtilitiesBuildingPipeline)}] No {typeof(T).Name} found in scene.");
                return null;
            }

            Debug.Log($"[{nameof(AnimatorUtilitiesBuildingPipeline)}] Found {components.Length} {typeof(T).Name} in scene.");

            return components;
        }
    }

}

#endif
