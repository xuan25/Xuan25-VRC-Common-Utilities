
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    public class PlayerVoiceController : UdonSharpBehaviour
    {
        #region Player Cache Management

        private VRCPlayerApi[] playersCache;

        private bool playersCacheInvalid = true;

        private VRCPlayerApi[] GetPlayerList()
        {
            if (playersCacheInvalid || playersCache == null || playersCache.Length != VRCPlayerApi.GetPlayerCount())
            {
                playersCache = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                VRCPlayerApi.GetPlayers(playersCache);
                playersCacheInvalid = false;
            }
            return playersCache;
        }

        private void InvalidatePlayerCache()
        {
            playersCacheInvalid = true;
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            InvalidatePlayerCache();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            InvalidatePlayerCache();
        }

        #endregion

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
            if (!Utilities.IsValid(player)) return;

            float gainScaler = 1.0f;
            float distanceNearScaler = 1.0f;
            float distanceFarScaler = 1.0f;
            float volumetricRadiusScaler = 1.0f;
            bool lowpassDisableMask = false;

            for (int i = 0; i < playerVoiceScalers.Length; i++)
            {
                playerVoiceScalers[i].GetPlayerVoiceScaler(player, out float gainScalerLocal, out float distanceNearScalerLocal, out float distanceFarScalerLocal, out float volumetricRadiusScalerLocal, out bool lowpassDisableMaskLocal, out bool scalerOverrideLocal);

                if (scalerOverrideLocal)
                {
                    gainScaler = gainScalerLocal;
                    distanceNearScaler = distanceNearScalerLocal;
                    distanceFarScaler = distanceFarScalerLocal;
                    volumetricRadiusScaler = volumetricRadiusScalerLocal;
                    lowpassDisableMask = lowpassDisableMaskLocal;
                    continue;
                }

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

#if DEBUG
            Debug.Log($"[PlayerVoiceController] Player: {player.displayName}, Gain: {gain}, DistanceNear: {distanceNear}, DistanceFar: {distanceFar}, VolumetricRadius: {volumetricRadius}, LowpassDisable: {lowpassDisable}");
#endif
        }

        public void UpdateAllPlayerVoice()
        {
            VRCPlayerApi[] players = GetPlayerList();
            for (int i = 0; i < players.Length; i++)
            {
                VRCPlayerApi player = players[i];
                if (Utilities.IsValid(player) && !player.isLocal)
                {
                    UpdatePlayerVoice(player);
                }
            }
        }

    }
}

