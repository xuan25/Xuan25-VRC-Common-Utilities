
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PlayerVoiceSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class PlayerVoiceScaler : UdonSharpBehaviour
    {
        internal PlayerVoiceController playerVoiceController;

        public void Setup(PlayerVoiceController controller)
        {
            playerVoiceController = controller;
        }

        public abstract void GetPlayerVoiceScaler(VRCPlayerApi player, out float gainScaler, out float distanceNearScaler, out float distanceFarScaler, out float volumetricRadiusScaler, out bool lowpassDisable);
    }
}