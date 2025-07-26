
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVoiceTrigger : PlayerVoiceScaler
    {
        #region Event source for external listeners

        [SerializeField]
        protected UdonSharpBehaviour[] targets;

        public void _AddListener(UdonSharpBehaviour callback)
        {
            if (!Utilities.IsValid(callback)) return;
            if (!Utilities.IsValid(targets))
            {
                targets = new UdonSharpBehaviour[] { callback };
                return;
            }
            if (System.Array.IndexOf(targets, callback) >= 0) return;
            var temp = new UdonSharpBehaviour[targets.Length + 1];
            System.Array.Copy(targets, temp, targets.Length);
            temp[targets.Length] = callback;
            targets = temp;
        }

        protected void SendEvent(string name)
        {
            if (!Utilities.IsValid(targets)) return;
            Debug.Log($"[{GetUdonTypeName()}] Send Event {name}");
            foreach (var ub in targets) if (Utilities.IsValid(ub)) ub.SendCustomEvent(name);
        }

        #endregion

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

        #endregion

        #region Voice Trigger

        public bool scalerOverrideIfTriggered = false;
        public float gainScaler = 1.5f;
        public float distanceNearScaler = 1.0f;
        public float distanceFarScaler = 5.0f;
        public float volumetricRadiusScaler = 1.0f;
        public bool lowpassDisable = false;

        public bool isEnabledLocal = true;

        [SerializeField]
        private bool activeUpdate = false;

        [HideInInspector]
        public bool[] playerMask = new bool[80];

        private bool[] playerMaskDirty;

        public override void Setup(PlayerVoiceController controller)
        {
            base.Setup(controller);

            for (int i = 0; i < playerMask.Length; i++)
            {
                playerMask[i] = false;
            }
            playerMaskDirty = new bool[playerMask.Length];
            Array.Copy(playerMask, playerMaskDirty, playerMask.Length);
        }

        void Start()
        {

        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (playerVoiceController == null)
            {
                Debug.LogError("[PlayerVoiceTriggerZone] PlayerVoiceController is not set up correctly.");
                return;
            }

            playerMask[player.playerId] = true;

            playerVoiceController.UpdatePlayerVoice(player);
            SendEvent("OnPlayerStateChanged");
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (playerVoiceController == null)
            {
                Debug.LogError("[PlayerVoiceTriggerZone] PlayerVoiceController is not set up correctly.");
                return;
            }

            playerMask[player.playerId] = false;

            playerVoiceController.UpdatePlayerVoice(player);
            SendEvent("OnPlayerStateChanged");
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (playerVoiceController == null)
            {
                Debug.LogError("[PlayerVoiceTriggerZone] PlayerVoiceController is not set up correctly.");
                return;
            }

            playerMaskDirty[player.playerId] = true;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            playerMask[player.playerId] = false;
            playerMaskDirty[player.playerId] = false;

            InvalidatePlayerCache();

            SendEvent("OnPlayerListChanged");
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            playerMask[player.playerId] = false;
            playerMaskDirty[player.playerId] = false;

            InvalidatePlayerCache();

            SendEvent("OnPlayerListChanged");
        }

        void FixedUpdate()
        {
            if (!activeUpdate) return;

            VRCPlayerApi[] vRCPlayerApis = GetPlayerList();
            for (int i = 0; i < vRCPlayerApis.Length; i++)
            {
                VRCPlayerApi player = vRCPlayerApis[i];
                if (!Utilities.IsValid(player))
                    continue;
                int playerId = player.playerId;
                if (playerMaskDirty[playerId] != playerMask[playerId])
                {
                    if (playerMaskDirty[playerId])
                    {
                        playerMask[playerId] = true;
                    }
                    else
                    {
                        playerMask[playerId] = false;
                    }
                    playerVoiceController.UpdatePlayerVoice(player);
                    SendEvent("OnPlayerStateChanged");
                }
                // Reset dirty state after processing, awaiting next FixedUpdate to set it again
                playerMaskDirty[playerId] = false;
            }
        }

        private void OnStateChanged()
        {
            for (int i = 0; i < playerMask.Length; i++)
            {
                if (playerMask[i])
                {
                    VRCPlayerApi player = VRCPlayerApi.GetPlayerById(i);
                    if (player != null)
                    {
                        playerVoiceController.UpdatePlayerVoice(player);
                    }
                }
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

        public override void GetPlayerVoiceScaler(VRCPlayerApi player, out float gainScaler, out float distanceNearScaler, out float distanceFarScaler, out float volumetricRadiusScaler, out bool lowpassDisable, out bool scalerOverride)
        {
            if (!isEnabledLocal || !playerMask[player.playerId])
            {
                gainScaler = 1.0f;
                distanceNearScaler = 1.0f;
                distanceFarScaler = 1.0f;
                volumetricRadiusScaler = 1.0f;
                lowpassDisable = false;
                scalerOverride = false;
                return;
            }

            gainScaler = this.gainScaler;
            distanceNearScaler = this.distanceNearScaler;
            distanceFarScaler = this.distanceFarScaler;
            volumetricRadiusScaler = this.volumetricRadiusScaler;
            lowpassDisable = this.lowpassDisable;
            scalerOverride = this.scalerOverrideIfTriggered;
        }
        
        #endregion

    }

}