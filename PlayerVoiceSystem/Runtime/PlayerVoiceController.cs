
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PlayerVoiceSystem {

    public class PlayerVoiceController : UdonSharpBehaviour
    {
        public PlayerVoiceScaler[] playerVoiceScalers;

        public float defaultGain = 15.0f;
        public float defaultDistanceNear = 0.0f;
        public float defaultDistanceFar = 25.0f;
        public float defaultVolumetricRadius = 0.0f;
        public bool defaultLowpassDisable = false;

        public void Start()
        {
            for (int i = 0; i < playerVoiceScalers.Length; i++)
            {
                if (playerVoiceScalers[i] != null)
                {
                    playerVoiceScalers[i].Setup(this);
                }
            }
        }
        
        public void UpdatePlayerVoice(VRCPlayerApi player)
        {
            if (player == null) return;

            float gainScaler = 1.0f;
            float distanceNearScaler = 1.0f;
            float distanceFarScaler = 1.0f;
            float volumetricRadiusScaler = 1.0f;
            bool lowpassDisableMask = false;

            for (int i = 0; i < playerVoiceScalers.Length; i++)
            {
                playerVoiceScalers[i].GetPlayerVoiceScaler(player, out float gainScalerLocal, out float distanceNearScalerLocal, out float distanceFarScalerLocal, out float volumetricRadiusScalerLocal, out bool lowpassDisableMaskLocal);

                gainScaler *= gainScalerLocal;
                distanceNearScaler *= distanceNearScalerLocal;
                distanceFarScaler *= distanceFarScalerLocal;
                volumetricRadiusScaler *= volumetricRadiusScalerLocal;
                lowpassDisableMask |= lowpassDisableMaskLocal;
            }

            float gain = defaultGain * gainScaler;
            float distanceNear = defaultDistanceNear * distanceNearScaler;
            float distanceFar = defaultDistanceFar * distanceFarScaler;
            float volumetricRadius = defaultVolumetricRadius * volumetricRadiusScaler;
            bool lowpassDisable = defaultLowpassDisable || lowpassDisableMask;

            player.SetVoiceDistanceFar(distanceFar);
            player.SetVoiceDistanceNear(distanceNear);
            player.SetVoiceGain(gain);
            player.SetVoiceVolumetricRadius(volumetricRadius);
            player.SetVoiceLowpass(!lowpassDisable);

            Debug.Log($"[PlayerVoiceController] Player: {player.displayName}, Gain: {gain}, DistanceNear: {distanceNear}, DistanceFar: {distanceFar}, VolumetricRadius: {volumetricRadius}, LowpassDisable: {lowpassDisable}");

        }

        public void UpdateAllPlayerVoice()
        {
            for (int i = 0; i < playerVoiceScalers.Length; i++)
            {
                VRCPlayerApi player = VRCPlayerApi.GetPlayerById(i);
                if (player != null)
                {
                    UpdatePlayerVoice(player);
                }
            }
        }
    }
    
}


