
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.UdonTelemetry
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UdonTelemetryEndpoint : UdonSharpBehaviour
    {
        [SerializeField]
        public VRCUrl[] urls;

        public VRCUrl GetUrl(int id)
        {
            if (id < 0 || id >= urls.Length)
                return null;
            return urls[id];
        }
    }
}