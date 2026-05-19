
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.Internationalization
{
    public abstract class ComponentLocale : UdonSharpBehaviour
    {
        [SerializeField]
        [Tooltip("The LocaleManager instance to register this component with.")]
        public LocaleManager manager;

        [SerializeField]
        [Tooltip("The text ID to look up in the locale file. If left empty, the component will use the current text as the text ID.")]
        public string textID = "";

        [SerializeField]
        [Tooltip("Optional variables to format the localized text with. The content and order of these variables are determined by the locale file.")]
        public string[] variables;

        public abstract bool EnsureTextID();

        public abstract void OnLocaleUpdated(string text);

        public void OnEnable()
        {
            if (manager == null)
            {
                Debug.LogError($"[{nameof(ComponentLocale)}] No LocaleManager configured. GameObject: {gameObject.name}");
                return;
            }
            manager.RegisterComponentLocale(this);
        }

        public void OnDisable()
        {
            if (manager == null)
            {
                Debug.LogError($"[{nameof(ComponentLocale)}] No LocaleManager configured. GameObject: {gameObject.name}");
                return;
            }
            manager.UnregisterComponentLocale(this);
        }

        public bool EnsureLocaleManager()
        {
            if (manager != null)
            {
                return true;
            }

            LocaleManager detectedManager = GetComponent<LocaleManager>();
            if (detectedManager == null)
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] No LocaleManager found in the scene. GameObject: {gameObject.name}");
                return false;
            }
            Register(detectedManager);
            return true;
        }

        public void Register(LocaleManager localeManager)
        {
            manager = localeManager;
            localeManager.RegisterComponentLocale(this);
        }

        public void OnLocaleUpdated()
        {
            if (!EnsureLocaleManager())
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] Failed to initialize. GameObject: {gameObject.name}");
                return;
            }

            if (!EnsureTextID())
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] Failed to ensure text ID. GameObject: {gameObject.name}");
                return;
            }

            if (!manager.GetText(textID, out string text))
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] Failed to get translation for text ID: {textID}. GameObject: {gameObject.name}");
                return;
            }

            if (variables != null)
            {
                text = string.Format(text, variables);
            }

            OnLocaleUpdated(text);
        }
    }
}