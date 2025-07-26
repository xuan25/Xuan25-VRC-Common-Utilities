
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVoiceRoomController : PlayerVoiceScaler
    {

        #region Event System for external listeners

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

        #region Player Voice Room Management

        public bool scalerOverrideIfIsolated = false;
        public float gainScaler = 1.0f;
        public float distanceNearScaler = 1.0f;
        public float distanceFarScaler = 0.01f;
        public float volumetricRadiusScaler = 1.0f;
        public bool lowpassDisable = false;

        public PlayerVoiceRoom[] playerVoiceRooms = new PlayerVoiceRoom[sizeof(int) * 8]; // 32 rooms maximum

        [HideInInspector]
        public int[] playerVoiceRoomMask = new int[80];

        private VRCPlayerApi localPlayer;

        void Start()
        {
            localPlayer = Networking.LocalPlayer;

            for (int i = 0; i < playerVoiceRoomMask.Length; i++)
            {
                playerVoiceRoomMask[i] = 0;
            }

            for (int i = 0; i < playerVoiceRooms.Length; i++)
            {
                if (playerVoiceRooms[i] != null)
                {
                    playerVoiceRooms[i].Setup(this, i);
                }
            }
        }

        public void OnPlayerRoomEnter(VRCPlayerApi player, int roomId)
        {
            playerVoiceRoomMask[player.playerId] |= 1 << roomId;
            OnPlayerRoomChanged(player);
        }

        public void OnPlayerRoomLeave(VRCPlayerApi player, int roomId)
        {
            playerVoiceRoomMask[player.playerId] &= ~(1 << roomId);
            OnPlayerRoomChanged(player);
        }

        private void OnPlayerRoomChanged(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;

            if (player.playerId == localPlayer.playerId)
            {
                playerVoiceController.UpdateAllPlayerVoice();
                SendEvent("OnPlayerStateChanged");
                return;
            }

            playerVoiceController.UpdatePlayerVoice(player);
            SendEvent("OnPlayerStateChanged");
        }

        private void ResetPlayerRoomMask(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            playerVoiceRoomMask[player.playerId] = 0;

            SendEvent("OnPlayerStateChanged");
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ResetPlayerRoomMask(player);
            SendEvent("OnPlayerListChanged");
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            ResetPlayerRoomMask(player);
            SendEvent("OnPlayerListChanged");
        }

        public override void GetPlayerVoiceScaler(VRCPlayerApi player, out float gainScaler, out float distanceNearScaler, out float distanceFarScaler, out float volumetricRadiusScaler, out bool lowpassDisable, out bool scalerOverride)
        {
            int localPlayerRoomMask = playerVoiceRoomMask[localPlayer.playerId];
            int targetPlayerId = player.playerId;

            if ((playerVoiceRoomMask[targetPlayerId] & localPlayerRoomMask) != 0 ||
                (playerVoiceRoomMask[targetPlayerId] == 0 && localPlayerRoomMask == 0))
            {
                // Player is in the same room as the local player or both are not in any room
                gainScaler = 1.0f;
                distanceNearScaler = 1.0f;
                distanceFarScaler = 1.0f;
                volumetricRadiusScaler = 1.0f;
                lowpassDisable = false;
                scalerOverride = false;
                return;
            }

            // Player is not in the same room as the local player
            gainScaler = this.gainScaler;
            distanceNearScaler = this.distanceNearScaler;
            distanceFarScaler = this.distanceFarScaler;
            volumetricRadiusScaler = this.volumetricRadiusScaler;
            lowpassDisable = this.lowpassDisable;
            scalerOverride = this.scalerOverrideIfIsolated;

        }
        
        #endregion

    }

}