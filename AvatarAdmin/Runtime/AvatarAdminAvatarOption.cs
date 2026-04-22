using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AvatarAdmin
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarAdminAvatarOption : UdonSharpBehaviour
    {
        [SerializeField]
        private string description;

        [SerializeField]
        private VRCAvatarPedestal pedestal;

        public string GetDescription()
        {
            return description;
        }
        
        public void SetAvatarUse()
        {
            pedestal.SetAvatarUse(Networking.LocalPlayer);
        }
    }
}