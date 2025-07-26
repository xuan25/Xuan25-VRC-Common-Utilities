using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerVoiceMic : PlayerVoiceScaler
    {
        public bool scalerOverrideIfHeld = false;
        public float gainScaler = 1.5f;
        public float distanceNearScaler = 1.0f;
        public float distanceFarScaler = 5.0f;
        public float volumetricRadiusScaler = 1.0f;
        public bool lowpassDisable = false;

        public bool isEnabledLocal = true;

        public Animator animator;

        private readonly string ANIMATOR_PARAM_HOLD_OTHER = "HeldOther";
        
        private readonly string ANIMATOR_PARAM_HOLD_SELF = "HeldSelf";

        [UdonSynced]
        private bool isHeld = false;
        
        
        void Start()
        {

        }

        public override void OnPickup()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            isHeld = true;
            if (animator != null) {
                animator.SetBool(ANIMATOR_PARAM_HOLD_SELF, isHeld);
            }
            RequestSerialization();
        }

        public override void OnDrop()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            isHeld = false;
            if (animator != null) {
                animator.SetBool(ANIMATOR_PARAM_HOLD_SELF, isHeld);
            }
            RequestSerialization();
        }

        private void OnStateChanged()
        {
            VRCPlayerApi ownerPlayer = Networking.GetOwner(this.gameObject);
            playerVoiceController.UpdatePlayerVoice(ownerPlayer);
            if (animator != null) {
                animator.SetBool(ANIMATOR_PARAM_HOLD_OTHER, isHeld);
            }
        }

        public void EnableLocal()
        {
            isEnabledLocal = true;

            OnStateChanged();
        }

        public void DisableLocal()
        {
            isEnabledLocal = false;

            OnStateChanged();
        }

        public override void OnDeserialization()
        {
            if (!isEnabledLocal) {
                return;
            }
            OnStateChanged();
        }

        public override void GetPlayerVoiceScaler(VRCPlayerApi player, out float gainScaler, out float distanceNearScaler, out float distanceFarScaler, out float volumetricRadiusScaler, out bool lowpassDisable, out bool scalerOverride)
        {
            VRCPlayerApi ownerPlayer = Networking.GetOwner(this.gameObject);
            if (!isEnabledLocal || !isHeld || ownerPlayer.playerId != player.playerId)
            {
                // Mic is not enabled, not held, or not owned by the player
                gainScaler = 1.0f;
                distanceNearScaler = 1.0f;
                distanceFarScaler = 1.0f;
                volumetricRadiusScaler = 1.0f;
                lowpassDisable = false;
                scalerOverride = false;
                return;
            }

            // Mic is enabled, held, and owned by the player
            gainScaler = this.gainScaler;
            distanceNearScaler = this.distanceNearScaler;
            distanceFarScaler = this.distanceFarScaler;
            volumetricRadiusScaler = this.volumetricRadiusScaler;
            lowpassDisable = this.lowpassDisable;
            scalerOverride = this.scalerOverrideIfHeld;
        }

    }

}