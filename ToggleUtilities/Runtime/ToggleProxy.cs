
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtilities
{
    public class ToggleProxy : UdonSharpBehaviour
    {
        public InteractToggleBase target;

        public override void Interact()
        {
            target.Interact();
        }
    }
}