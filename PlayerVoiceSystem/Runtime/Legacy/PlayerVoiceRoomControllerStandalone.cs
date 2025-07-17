
// using UdonSharp;
// using UnityEngine;
// using VRC.SDKBase;
// using VRC.Udon;

// namespace PlayerVoiceSystem
// {

//     [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
//     public class PlayerVoiceRoomControllerStandalone : UdonSharpBehaviour
//     {
//         public float defaultGain = 15.0f;
//         public float gainScaler = 0.01f;

//         public PlayerVoiceRoomStandalone[] playerVoiceRooms = new PlayerVoiceRoomStandalone[32];

//         private int[] playerVoiceRoomMask = new int[80];
        
//         void Start()
//         {
//             for (int i = 0; i < playerVoiceRoomMask.Length; i++)
//             {
//                 playerVoiceRoomMask[i] = 0;
//             }

//             for (int i = 0; i < playerVoiceRooms.Length; i++)
//             {
//                 if (playerVoiceRooms[i] != null)
//                 {
//                     playerVoiceRooms[i].Setup(this, i);
//                 }
//             }
//         }

//         public void PlayerRoomEnter(int playerId, int roomId)
//         {
//             playerVoiceRoomMask[playerId] |= 1 << roomId;
//             UpdatePlayerVoiceConfig();
//         }

//         public void PlayerRoomLeave(int playerId, int roomId)
//         {
//             playerVoiceRoomMask[playerId] &= ~(1 << roomId);
//             UpdatePlayerVoiceConfig();
//         }

//         private void UpdatePlayerVoiceConfig() {
//             int localPlayerId = Networking.LocalPlayer.playerId;
//             int localPlayerRoomMask = playerVoiceRoomMask[localPlayerId];

//             for (int i = 0; i < playerVoiceRoomMask.Length; i++)
//             {
//                 if (i == localPlayerId)
//                     continue;

//                 VRCPlayerApi remotePlayer = VRCPlayerApi.GetPlayerById(i);
//                 if (remotePlayer == null)
//                     continue;

//                 if ((playerVoiceRoomMask[i] & localPlayerRoomMask) != 0 || 
//                     (playerVoiceRoomMask[i] == 0 && localPlayerRoomMask == 0))
//                 {
//                     // Player is in the same room as the local player or both are not in any room
//                     remotePlayer.SetVoiceGain(defaultGain);
//                 }
//                 else
//                 {
//                     // Player is not in the same room as the local player
//                     remotePlayer.SetVoiceGain(defaultGain * gainScaler);
//                 }
//             }
//         }

//         public override void OnPlayerJoined(VRCPlayerApi player)
//         {
//             if (player == null) return;
//             playerVoiceRoomMask[player.playerId] = 0;
//         }

//         public override void OnPlayerLeft(VRCPlayerApi player)
//         {
//             if (player == null) return;
//             playerVoiceRoomMask[player.playerId] = 0;
//         }
//     }

// }