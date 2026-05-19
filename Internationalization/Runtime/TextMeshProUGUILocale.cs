
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
        
        public override void OnLocaleUpdated(string text)
        {
            if (!EnsureComponent())
            {
                Debug.LogError($"[{nameof(TextMeshProUGUILocale)}] No TextMeshProUGUI component found. GameObject: {gameObject.name}");
                return;
            }

            textComponent.text = text;
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

        public override bool EnsureTextID()
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
    }
}