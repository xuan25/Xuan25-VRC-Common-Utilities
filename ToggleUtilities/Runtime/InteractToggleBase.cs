
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtilities
{
    public abstract class InteractToggleBase : UdonSharpBehaviour
    {
        [SerializeField] protected bool isGlobal = false;

        public override void Interact()
        {
            OnToggle(isGlobal);
        }

        public abstract void OnToggle(bool global);
    }
}