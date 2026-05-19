
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.Internationalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class LocaleManager : UdonSharpBehaviour
    {
        [SerializeField]
        [Tooltip("The locale to use as a fallback if no matching locale is found.")]
        public string fallbackLocale = "";

        private LocaleHandle[] localeHandles;

        private ComponentLocale[] componentLocales;

        private LocaleHandle currentLocaleHandle;

        private string currentLocale = null;

        public void OnEnable()
        {
            string lang = VRCPlayerApi.GetCurrentLanguage();
            currentLocale = lang;
            Debug.Log($"[{nameof(LocaleManager)}] Detected language: {lang}. GameObject: {gameObject.name}");
            InvalidateLocale();
        }

        public void RegisterLocaleFile(LocaleHandle handle)
        {
            if (localeHandles == null)
            {
                localeHandles = new LocaleHandle[] { handle };
            }
            else
            {
                int length = localeHandles.Length;
                LocaleHandle[] newLocaleHandles = new LocaleHandle[length + 1];
                Array.Copy(localeHandles, newLocaleHandles, length);
                newLocaleHandles[length] = handle;
                localeHandles = newLocaleHandles;
            }

            handle.manager = this;

            if (currentLocale != null && handle.GetLanguageID(out string lang) && (lang == currentLocale || lang == fallbackLocale))
            {
                InvalidateLocale();
            }
        }
        
        public void UnregisterLocaleHandle(LocaleHandle handle)
        {
            if (localeHandles == null)
            {
                Debug.LogError($"[{nameof(LocaleManager)}] No locale handles registered. GameObject: {gameObject.name}");
                return;
            }

            int index = Array.IndexOf(localeHandles, handle);
            if (index < 0)
            {
                Debug.LogError($"[{nameof(LocaleManager)}] Locale handle not found during unregistration. GameObject: {gameObject.name}");
                return;
            }

            int length = localeHandles.Length;
            LocaleHandle[] newLocaleHandles = new LocaleHandle[length - 1];
            if (index > 0)
            {
                Array.Copy(localeHandles, 0, newLocaleHandles, 0, index);
            }
            if (index < length - 1)
            {
                Array.Copy(localeHandles, index + 1, newLocaleHandles, index, length - index - 1);
            }
            localeHandles = newLocaleHandles;

            handle.manager = null;

            if (currentLocale != null && handle.GetLanguageID(out string lang) && (lang == currentLocale || lang == fallbackLocale))
            {
                InvalidateLocale();
            }
        }

        public void RegisterComponentLocale(ComponentLocale componentLocale)
        {
            if (componentLocales == null)
            {
                componentLocales = new ComponentLocale[] { componentLocale };
            }
            else
            {
                int length = componentLocales.Length;
                ComponentLocale[] newComponentLocales = new ComponentLocale[length + 1];
                Array.Copy(componentLocales, newComponentLocales, length);
                newComponentLocales[length] = componentLocale;
                componentLocales = newComponentLocales;
            }

            componentLocale.manager = this;

            if (currentLocale != null)
            {
                componentLocale.OnLocaleUpdated();
            }
        }

        public void UnregisterComponentLocale(ComponentLocale componentLocale)
        {
            if (componentLocales == null)
            {
                Debug.LogError($"[{nameof(LocaleManager)}] No component locales registered. GameObject: {gameObject.name}");
                return;
            }

            int index = Array.IndexOf(componentLocales, componentLocale);
            if (index < 0)
            {
                Debug.LogError($"[{nameof(LocaleManager)}] Component locale not found during unregistration. GameObject: {gameObject.name}");
                return;
            }

            int length = componentLocales.Length;
            ComponentLocale[] newComponentLocales = new ComponentLocale[length - 1];
            if (index > 0)
            {
                Array.Copy(componentLocales, 0, newComponentLocales, 0, index);
            }
            if (index < length - 1)
            {
                Array.Copy(componentLocales, index + 1, newComponentLocales, index, length - index - 1);
            }
            componentLocales = newComponentLocales;

            componentLocale.manager = null;
        }

        public bool GetText(string id, out string msg)
        {
            if (localeHandles == null)
            {
                Debug.LogError($"[{nameof(LocaleManager)}] No locale handles registered. GameObject: {gameObject.name}");
                msg = id;
                return false;
            }

            if (currentLocaleHandle == null)
            {
                Debug.LogError($"[{nameof(LocaleManager)}] No current locale handle set. GameObject: {gameObject.name}");
                msg = id;
                return false;
            }

            if (!currentLocaleHandle.GetText(id, out string translation))
            {
                Debug.LogWarning($"[{nameof(LocaleManager)}] No translation found for key: {id}. GameObject: {gameObject.name}");
                msg = id;
                return false;
            }

            msg = translation;
            return true;
        }

        public override void OnLanguageChanged(string language)
        {
            Debug.Log($"[{nameof(LocaleManager)}] Language changed to: {language}. GameObject: {gameObject.name}");
            currentLocale = language;
            InvalidateLocale();
        }
        
        private void InvalidateLocale()
        {
            string language = currentLocale;

            if (localeHandles == null)
            {
                Debug.LogError($"[{nameof(LocaleManager)}] No locale handles registered. GameObject: {gameObject.name}");
                return;
            }

            LocaleHandle fallbackLocaleHandle = null;

            foreach (LocaleHandle handle in localeHandles)
            {
                if (handle.GetLanguageID(out string lang))
                {
                    if (lang == language)
                    {
                        currentLocaleHandle = handle;
                        break;
                    }
                    if (lang == fallbackLocale)
                    {
                        fallbackLocaleHandle = handle;
                    }
                }
            }

            if (currentLocaleHandle == null)
            {
                Debug.LogWarning($"[{nameof(LocaleManager)}] No locale handle found for language: {language}. GameObject: {gameObject.name}");
                if (fallbackLocaleHandle == null)
                {
                    Debug.LogError($"[{nameof(LocaleManager)}] No fallback locale handle found for language: {fallbackLocale}. GameObject: {gameObject.name}");
                    return;
                }
                Debug.LogWarning($"[{nameof(LocaleManager)}] Using fallback locale handle for language: {fallbackLocale}. GameObject: {gameObject.name}");
                currentLocaleHandle = fallbackLocaleHandle;
            }

            if (componentLocales == null || componentLocales.Length == 0)
            {
                Debug.LogWarning($"[{nameof(LocaleManager)}] No component locales registered. GameObject: {gameObject.name}");
                return;
            }

            foreach (ComponentLocale componentLocale in componentLocales)
            {
                componentLocale.OnLocaleUpdated();
            }

            Debug.Log($"[{nameof(LocaleManager)}] {componentLocales.Length} component locales updated. GameObject: {gameObject.name}");
        }
    }

}