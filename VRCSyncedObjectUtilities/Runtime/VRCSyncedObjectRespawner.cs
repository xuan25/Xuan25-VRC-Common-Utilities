
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.VRCSyncedObjectUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class VRCSyncedObjectRespawner : UdonSharpBehaviour
    {
        public VRCObjectSync[] vRCObjectSyncs;

        public override void Interact()
        {
            foreach (VRCObjectSync vrcObjectSync in vRCObjectSyncs)
            {
                if (vrcObjectSync != null)
                {
                    Networking.SetOwner(Networking.LocalPlayer, vrcObjectSync.gameObject);
                    vrcObjectSync.Respawn();
                }
            }
        }
    }
}
