
using UdonSharp;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.Internationalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class TextMeshProUGUILocale : ComponentLocale
    {
        [SerializeField]
        [Tooltip("The TextMeshProUGUI component to update. If left empty, the script will attempt to find one on the same GameObject.")]
        public TMPro.TextMeshProUGUI textComponent;

        [SerializeField]
        [Tooltip("The text ID to look up in the locale file. If left empty, the component will use the current text as the text ID.")]
        public string textID = "";
        
        public override void OnLocaleUpdated()
        {
            if (!EnsureInit())
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] Failed to initialize. GameObject: {gameObject.name}");
                return;
            }

            if (!localeManager.GetText(textID, out string msg))
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] Failed to get translation for text ID: {textID}. GameObject: {gameObject.name}");
                return;
            }

            textComponent.text = msg;
        }

        public bool EnsureComponent()
        {
            if (textComponent != null)
            {
                return true;
            }
            textComponent = GetComponent<TMPro.TextMeshProUGUI>();
            if (textComponent == null)
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] No TextMeshProUGUI component found on the GameObject. GameObject: {gameObject.name}");
                return false;
            }
            return true;
        }

        public bool EnsureTextID()
        {
            if (textID != "")
            {
                return true;
            }
            if (textComponent == null)
            {
                if (!EnsureComponent())
                {
                    Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] Failed to find TextMeshProUGUI component. GameObject: {gameObject.name}");
                    return false;
                }
            }
            textID = textComponent.text;
            return true;
        }

        public bool EnsureLocaleManager()
        {
            if (localeManager != null)
            {
                return true;
            }
            LocaleManager manager = GetComponent<LocaleManager>();
            if (manager == null)
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] No LocaleManager found in the scene. GameObject: {gameObject.name}");
                return false;
            }
            Register(manager);
            return true;
        }

        public bool EnsureInit()
        {
            if (!EnsureComponent())
                return false;

            if (!EnsureTextID())
                return false;

            if (!EnsureLocaleManager())
                return false;
            
            return true;
        }

        public void Register(LocaleManager localeManager)
        {
            this.localeManager = localeManager;
            localeManager.RegisterComponentLocale(this);
        }
    }
}