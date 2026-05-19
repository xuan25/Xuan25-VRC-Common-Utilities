
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

        public abstract void OnLocaleUpdated();
    }
}