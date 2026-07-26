
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVoiceMultipartRoom : PlayerVoiceRoomBase
    {
        [SerializeField]
        public PlayerVoiceRoom[] parts;
        
        private PlayerVoiceRoomController playerVoiceRoomController;

        private int roomID;

        public int[] playerVoiceRoomMask
        {
            get
            {
                return playerVoiceRoomController.playerVoiceRoomMask;
            }
        }

        public override void Setup(PlayerVoiceRoomController controller, int roomID)
        {
            this.playerVoiceRoomController = controller;
            this.roomID = roomID;

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].Setup(this, roomID << 8 | i);
            }
        }

        public void OnPlayerRoomEnter(VRCPlayerApi player, int compartmentID)
        {
            int playerId = player.playerId;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].playerMask[playerId] && (compartmentID & 0xFF) != i)
                {
                    // Player is already in this compartment, no need to enter again
                    Debug.Log($"[{nameof(PlayerVoiceMultipartRoom)}] Player {player.displayName} is entering the multi-compartment room {roomID} via compartment {compartmentID & 0xFF}, but is already in compartment {i}. Ignoring.");
                    return;
                }
            }
            if (playerVoiceRoomController == null)
            {
                Debug.LogError($"[{nameof(PlayerVoiceMultipartRoom)}] PlayerVoiceRoomController is not set.");
                return;
            }
            Debug.Log($"[{nameof(PlayerVoiceMultipartRoom)}] Player {player.displayName} is entering the multi-compartment room {roomID} via compartment {compartmentID & 0xFF}.");
            playerVoiceRoomController.OnPlayerRoomEnter(player, roomID);
        }

        public void OnPlayerRoomLeave(VRCPlayerApi player, int compartmentID)
        {
            int playerId = player.playerId;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].playerMask[playerId] && (compartmentID & 0xFF) != i)
                {
                    // Player is still in another compartment, no need to leave the room
                    Debug.Log($"[{nameof(PlayerVoiceMultipartRoom)}] Player {player.displayName} is leaving compartment {compartmentID & 0xFF} but is still in compartment {i}, not leaving the multi-compartment room {roomID}.");
                    return;
                }
            }
            if (playerVoiceRoomController == null)
            {
                Debug.LogError($"[{nameof(PlayerVoiceMultipartRoom)}] PlayerVoiceRoomController is not set.");
                return;
            }
            Debug.Log($"[{nameof(PlayerVoiceMultipartRoom)}] Player {player.displayName} is leaving the multi-compartment room {roomID} via compartment {compartmentID & 0xFF}.");
            playerVoiceRoomController.OnPlayerRoomLeave(player, roomID);
        }

    }

}