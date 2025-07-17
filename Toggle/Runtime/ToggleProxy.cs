
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtility
{
    public class ToggleProxy : UdonSharpBehaviour
    {
        public ToggleBase target;

        public override void Interact()
        {
            target.Interact();
        }
    }
}