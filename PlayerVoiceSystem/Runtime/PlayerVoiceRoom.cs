
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVoiceRoom : UdonSharpBehaviour
    {

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

            if (activeUpdate)
            {
                playerMask = new bool[controller.playerVoiceRoomMask.Length];
                for (int i = 0; i < playerMask.Length; i++)
                {
                    playerMask[i] = false;
                }
                playerMaskDirty = new bool[controller.playerVoiceRoomMask.Length];
                Array.Copy(playerMask, playerMaskDirty, playerMask.Length);
            }
        }

        void Start()
        {

        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (player == null) return;

            if (playerVoiceRoomController == null)
            {
                Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
                return;
            }

            if (activeUpdate)
            {
                playerMask[player.playerId] = true;
            }

            playerVoiceRoomController.OnPlayerRoomEnter(player, roomID);

        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (activeUpdate) return;
            if (player == null) return;

            if (playerVoiceRoomController == null)
            {
                Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
                return;
            }

            if (activeUpdate)
            {
                playerMask[player.playerId] = false;
            }

            playerVoiceRoomController.OnPlayerRoomLeave(player, roomID);
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (player == null) return;

            if (playerVoiceRoomController == null)
            {
                Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
                return;
            }

            if (activeUpdate)
            {
                playerMaskDirty[player.playerId] = true;
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (player == null) return;

            playerMask[player.playerId] = false;
            playerMaskDirty[player.playerId] = false;
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!activeUpdate) return;
            if (player == null) return;

            playerMask[player.playerId] = false;
            playerMaskDirty[player.playerId] = false;
        }

        void FixedUpdate()
        {
            if (!activeUpdate) return;

            VRCPlayerApi[] vRCPlayerApis = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(vRCPlayerApis);

            for (int i = 0; i < vRCPlayerApis.Length; i++)
            {
                VRCPlayerApi player = vRCPlayerApis[i];
                int playerId = player.playerId;
                if (playerMaskDirty[playerId] != playerMask[playerId])
                {
                    if (playerMaskDirty[playerId])
                    {
                        playerVoiceRoomController.OnPlayerRoomEnter(player, roomID);
                        playerMask[playerId] = true;
                    }
                    else
                    {
                        playerVoiceRoomController.OnPlayerRoomLeave(player, roomID);
                        playerMask[playerId] = false;
                    }
                }
                playerMaskDirty[playerId] = false;
            }
        }
    }

}