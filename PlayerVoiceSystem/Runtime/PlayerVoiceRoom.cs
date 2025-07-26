
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVoiceRoom : UdonSharpBehaviour
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

        #endregion

        #region Room Management

        private int roomID;

        private PlayerVoiceRoomController playerVoiceRoomController;

        [SerializeField]
        private bool activeUpdate = false;

        private bool[] playerMask;

        private bool[] playerMaskDirty;

        public void Setup(PlayerVoiceRoomController controller, int roomID)
        {
            this.playerVoiceRoomController = controller;
            this.roomID = roomID;

            playerMask = new bool[controller.playerVoiceRoomMask.Length];
            for (int i = 0; i < playerMask.Length; i++)
            {
                playerMask[i] = false;
            }
            playerMaskDirty = new bool[controller.playerVoiceRoomMask.Length];
            Array.Copy(playerMask, playerMaskDirty, playerMask.Length);
        }

        void Start()
        {

        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (playerVoiceRoomController == null)
            {
                Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
                return;
            }

            playerVoiceRoomController.OnPlayerRoomEnter(player, roomID);

        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (playerVoiceRoomController == null)
            {
                Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
                return;
            }

            playerVoiceRoomController.OnPlayerRoomLeave(player, roomID);
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (playerVoiceRoomController == null)
            {
                Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
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
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            playerMask[player.playerId] = false;
            playerMaskDirty[player.playerId] = false;

            InvalidatePlayerCache();
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
                        playerVoiceRoomController.OnPlayerRoomEnter(player, roomID);
                    }
                    else
                    {
                        playerMask[playerId] = false;
                        playerVoiceRoomController.OnPlayerRoomLeave(player, roomID);
                    }
                }
                // Reset dirty state after processing, awaiting next FixedUpdate to set it again
                playerMaskDirty[playerId] = false;
            }
        }

        #endregion
    }

}