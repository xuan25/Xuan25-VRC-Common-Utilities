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

        /// <summary>
        /// Get the text for the given text ID.
        /// 
        /// </summary>
        /// <param name="id">The text ID</param>
        /// <param name="text">The retrieved text</param>
        /// <returns>Whether the text was successfully retrieved</returns>
        public abstract bool GetText(string id, out string text);

        /// <summary>
        /// Get the language ID for this locale handle in RFC 5646 format.
        /// 
        /// </summary>
        /// <param name="language">The language ID in RFC 5646 format</param>
        /// <returns>Whether the language ID was successfully retrieved</returns>
        public abstract bool GetLanguageID(out string language);

        /// <summary>
        /// Bake the locale file to prepare it for use.
        /// This method will be called during the build process for handles to prepare reusable data structures for runtime use.
        /// This is for optimization purposes, as some handles may require expensive parsing or setup that can be done ahead of time and reused at runtime.
        /// For example, a handle that loads translations from a file may parse the file and store the translations in a dictionary for lookup at runtime.
        /// 
        /// </summary>
        /// <returns>Whether the bake process was successful</returns>
        public abstract bool Bake();

#if UNITY_EDITOR

        /// <summary>
        /// Reset the locale file to its initial state.
        /// This will be called when Inspector need to be updated.
        /// When this method is called, the internal state of the handle should be reset to prevent stale data from being used.
        /// 
        /// </summary>
        public abstract void Reset();
#endif

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

    }
}
