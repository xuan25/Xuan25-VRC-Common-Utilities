
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVoiceRoom : PlayerVoiceRoomBase
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
        private PlayerVoiceMultipartRoom multipartRoom;

        #region Controller Dynamic Dispatching

        // Implement dynamic dispatching manually because UdonSharp does not support interfaces.
        // playerVoiceRoomController has already derived from an abstract class that mimics the interface, 
        // which prevents the class further derived from another abstract class that mimics another interface.

        private void Controller_Assign_DP(PlayerVoiceRoomController controller)
        {
            this.multipartRoom = null;
            this.playerVoiceRoomController = controller;
        }

        private void Controller_Assign_DP(PlayerVoiceMultipartRoom controller)
        {
            this.playerVoiceRoomController = null;
            this.multipartRoom = controller;
        }

                // Wrapper method is required due to manual implementation of dynamic dispatching.
        private bool Controller_IsNull_DP()
        {
            return playerVoiceRoomController == null && multipartRoom == null;
        }

        private void Controller_OnPlayerRoomEnter_DP(VRCPlayerApi player, int roomId)
        {
            if (playerVoiceRoomController != null)
            {
                playerVoiceRoomController.OnPlayerRoomEnter(player, roomID);
            }
            if (multipartRoom != null)
            {
                multipartRoom.OnPlayerRoomEnter(player, roomID);
            }
        }

        private void Controller_OnPlayerRoomLeave_DP(VRCPlayerApi player, int roomId)
        {
            if (playerVoiceRoomController != null)
            {
                playerVoiceRoomController.OnPlayerRoomLeave(player, roomID);
            }
            if (multipartRoom != null)
            {
                multipartRoom.OnPlayerRoomLeave(player, roomID);
            }
        }

        #endregion

        [SerializeField]
        private bool activeUpdate = false;

        [NonSerialized]
        public bool[] playerMask;

        private bool[] playerMaskDirty;

        public override void Setup(PlayerVoiceRoomController controller, int roomID)
        {
            Controller_Assign_DP(controller);
            this.roomID = roomID;

            playerMask = new bool[controller.playerVoiceRoomMask.Length];
            for (int i = 0; i < playerMask.Length; i++)
            {
                playerMask[i] = false;
            }
            playerMaskDirty = new bool[controller.playerVoiceRoomMask.Length];
            Array.Copy(playerMask, playerMaskDirty, playerMask.Length);
        }

        public void Setup(PlayerVoiceMultipartRoom controller, int roomID)
        {
            Controller_Assign_DP(controller);
            this.roomID = roomID;

            playerMask = new bool[controller.playerVoiceRoomMask.Length];
            for (int i = 0; i < playerMask.Length; i++)
            {
                playerMask[i] = false;
            }
            playerMaskDirty = new bool[controller.playerVoiceRoomMask.Length];
            Array.Copy(playerMask, playerMaskDirty, playerMask.Length);
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (Controller_IsNull_DP())
            {
                Debug.LogError($"[{nameof(PlayerVoiceRoom)}] Controller is not set up correctly.");
                return;
            }

            Controller_OnPlayerRoomEnter_DP(player, roomID);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (Controller_IsNull_DP())
            {
                Debug.LogError($"[{nameof(PlayerVoiceRoom)}] Controller is not set up correctly.");
                return;
            }

            Controller_OnPlayerRoomLeave_DP(player, roomID);
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (!Utilities.IsValid(player)) return;

            if (Controller_IsNull_DP())
            {
                Debug.LogError($"[{nameof(PlayerVoiceRoom)}] Controller is not set up correctly.");
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
                        Controller_OnPlayerRoomEnter_DP(player, roomID);
                    }
                    else
                    {
                        playerMask[playerId] = false;
                        Controller_OnPlayerRoomLeave_DP(player, roomID);
                    }
                }
                // Reset dirty state after processing, awaiting next FixedUpdate to set it again
                playerMaskDirty[playerId] = false;
            }
        }

        #endregion
    }

}