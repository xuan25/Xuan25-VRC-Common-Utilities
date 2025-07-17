
// using UdonSharp;
// using UnityEngine;
// using VRC.SDKBase;
// using VRC.Udon;

// namespace PlayerVoiceSystem
// {

//     [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
//     public class PlayerVoiceRoomStandalone : UdonSharpBehaviour
//     {
//         private int roomID;

//         private PlayerVoiceRoomControllerStandalone playerVoiceRoomController;

//         public void Setup(PlayerVoiceRoomControllerStandalone controller, int roomID)
//         {
//             this.playerVoiceRoomController = controller;
//             this.roomID = roomID;
//         }

//         void Start()
//         {

//         }

//         public override void OnPlayerTriggerEnter(VRCPlayerApi player)
//         {
//             if (player == null) return;

//             if (playerVoiceRoomController == null)
//             {
//                 Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
//                 return;
//             }
            
//             playerVoiceRoomController.PlayerRoomEnter(player.playerId, roomID);
//         }

//         public override void OnPlayerTriggerExit(VRCPlayerApi player)
//         {
//             if (player == null) return;

//             if (playerVoiceRoomController == null)
//             {
//                 Debug.LogError("[PlayerVoiceRoom] PlayerVoiceRoomController is not set up correctly.");
//                 return;
//             }
            
//             playerVoiceRoomController.PlayerRoomLeave(player.playerId, roomID);
//         }
//     }

// }