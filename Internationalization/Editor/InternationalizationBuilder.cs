#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Xuan25.Internationalization.Editor
{
    
    public class InternationalizationBuilder : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private LocaleManager GetFallbackLocaleManager() => 
            FindComponentGlobalFirst<LocaleManager>();

        private PortableObjectHandle[] GetPortableObjectHandles() =>
            FindComponentGlobal<PortableObjectHandle>();

        private ComponentLocale[] GetComponentLocale() =>
            FindComponentGlobal<ComponentLocale>();

        private TextMeshProUGUILocale[] GetTextMeshProUGUILocales() =>
            FindComponentGlobal<TextMeshProUGUILocale>();

        private void BakePortableObjectHandles(PortableObjectHandle[] handles)
        {
            if (handles == null) return;

            int count = 0;
            foreach (PortableObjectHandle handle in handles)
            {
                if (!handle.Bake())
                    continue;
                count++;
            }

            Debug.Log($"[{nameof(InternationalizationBuilder)}] Baked {count} / {handles.Length} PortableObjectHandles.");
        }

        private void HookupPortableObjectHandles(PortableObjectHandle[] handles, LocaleManager fallbackLocaleManager)
        {
            if (handles == null || fallbackLocaleManager == null) return;

            int count = 0;
            foreach (PortableObjectHandle handle in handles)
            {
                if (handle.manager != null)
                    continue;
                fallbackLocaleManager.RegisterLocaleFile(handle);
                count++;
            }

            Debug.Log($"[{nameof(InternationalizationBuilder)}] Hooked up {count} / {handles.Length} PortableObjectHandles to fallback LocaleManager. GameObject: {fallbackLocaleManager.gameObject.name}");
        }

        private void HookupComponentLocales(ComponentLocale[] componentLocales, LocaleManager fallbackLocaleManager)
        {
            if (componentLocales == null || fallbackLocaleManager == null) return;

            int count = 0;
            foreach (ComponentLocale componentLocale in componentLocales)
            {
                if (componentLocale.manager != null)
                    continue;
                fallbackLocaleManager.RegisterComponentLocale(componentLocale);
                count++;
            }

            Debug.Log($"[{nameof(InternationalizationBuilder)}] Hooked up {count} / {componentLocales.Length} ComponentLocales to fallback LocaleManager. GameObject: {fallbackLocaleManager.gameObject.name}");
        }

        private void HookupTextMeshProUGUI(TextMeshProUGUILocale[] textMeshProUGUILocales)
        {
            if (textMeshProUGUILocales == null) return;

            int count = 0;
            int failed = 0;
            foreach (TextMeshProUGUILocale textMeshProUGUILocale in textMeshProUGUILocales)
            {
                if (textMeshProUGUILocale.textComponent != null)
                    continue;
                TMPro.TextMeshProUGUI target = textMeshProUGUILocale.GetComponent<TMPro.TextMeshProUGUI>();
                if (target == null)
                    failed++;

                textMeshProUGUILocale.textComponent = target;
                count++;
            }

            Debug.Log($"[{nameof(InternationalizationBuilder)}] Hooked up {count} / {textMeshProUGUILocales.Length} TextMeshProUGUILocale text components. Failed to hook up {failed} TextMeshProUGUILocale text components.");
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log($"[{nameof(InternationalizationBuilder)}] Processing scene: " + scene.name);

            LocaleManager fallbackLocaleManager = GetFallbackLocaleManager();
            if (fallbackLocaleManager == null)
                return;
            
            PortableObjectHandle[] portableObjectHandles = GetPortableObjectHandles();
            ComponentLocale[] componentLocales = GetComponentLocale();
            TextMeshProUGUILocale[] textMeshProUGUILocales = GetTextMeshProUGUILocales();

            BakePortableObjectHandles(portableObjectHandles);
            HookupPortableObjectHandles(portableObjectHandles, fallbackLocaleManager);
            HookupComponentLocales(componentLocales, fallbackLocaleManager);
            HookupTextMeshProUGUI(textMeshProUGUILocales);
        }

#region Utility

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
                Debug.Log($"[{nameof(InternationalizationBuilder)}] No {typeof(T).Name} found in scene.");
                return null;
            }

            Debug.Log($"[{nameof(InternationalizationBuilder)}] Found {components.Length} {typeof(T).Name} in scene.");

            return components;
        }
    }

#endregion

}

#endif
