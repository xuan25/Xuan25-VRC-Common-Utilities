
// using UdonSharp;
// using UnityEngine;
// using VRC.SDKBase;
// using VRC.Udon;

// namespace PlayerVoiceSystem
// {

//     [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
//     public class PlayerVoiceMicStandalone : UdonSharpBehaviour
//     {

//         public float defaultRange = 25.0f;
//         public float rangeScaler = 5.0f;
//         public bool isEnabledLocal = true;

//         [UdonSynced]
//         private bool isHeld = false;
        
//         void Start()
//         {

//         }

//         public override void OnPickup()
//         {
//             Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
//             isHeld = true;
//             RequestSerialization();
//         }

//         public override void OnDrop()
//         {
//             Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
//             isHeld = false;
//             RequestSerialization();
//         }

//         public void EnableLocal() {
//             isEnabledLocal = true;
//             UpdateVoiceConfig();
//         }

//         public void DisableLocal() {
//             isEnabledLocal = false;
//             VRCPlayerApi ownerPlayer = Networking.GetOwner(this.gameObject);
//             ownerPlayer.SetVoiceDistanceFar(defaultRange);
//         }

//         private void UpdateVoiceConfig() {
//             if (!isEnabledLocal) {
//                 return;
//             }
//             VRCPlayerApi ownerPlayer = Networking.GetOwner(this.gameObject);
//             if (isHeld)
//             {
//                 ownerPlayer.SetVoiceDistanceFar(defaultRange * rangeScaler);
//             }
//             else
//             {
//                 ownerPlayer.SetVoiceDistanceFar(defaultRange);
//             }
//         }

//         public override void OnDeserialization()
//         {
//             UpdateVoiceConfig();
//         }
//     }

// }