
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class PlayerVoiceMicPickupProxy : UdonSharpBehaviour
    {
        public PlayerVoiceMic playerVoiceMic;
        void Start()
        {
            
        }

        public override void OnPickup()
        {
            if (playerVoiceMic != null)
            {
                playerVoiceMic.OnPickup();
            }
        }

        public override void OnDrop()
        {
            if (playerVoiceMic != null)
            {
                playerVoiceMic.OnDrop();
            }
        }
    }

}