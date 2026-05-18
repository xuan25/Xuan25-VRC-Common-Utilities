
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.Internationalization
{
    public abstract class LocaleHandle : UdonSharpBehaviour
    {
        [SerializeField]
        [Tooltip("The LocaleManager instance to register this locale file with.")]
        public LocaleManager manager;

        public void OnEnable()
        {
            if (manager == null)
            {
                Debug.LogError($"[{nameof(LocaleHandle)}] No LocaleManager found in the scene. GameObject: {gameObject.name}");
                return;
            }
            manager.RegisterLocaleFile(this);
        }

        public void OnDisable()
        {
            if (manager == null)
            {
                Debug.LogError($"[{nameof(LocaleHandle)}] No LocaleManager found in the scene. GameObject: {gameObject.name}");
                return;
            }
            manager.UnregisterLocaleHandle(this);
        }

        public abstract bool GetText(string id, out string msg);

        public abstract bool GetLanguageID(out string language);

    }
}
