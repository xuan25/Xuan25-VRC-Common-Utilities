
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace Hints
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HintController : UdonSharpBehaviour
    {
        public string hintKey = "DefaultHint";

        public bool showInPCVR = true;
        public bool showInPCDesktop = true;
        public bool showInAndroidVR = true;
        public bool showInAndroidMobile = true;
        public bool showInIOS = true;
        public bool showInVisionOS = true;

        void Start()
        {
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            if (Networking.LocalPlayer.IsUserInVR()) {
                if(!showInPCVR)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                if(!showInPCDesktop)
                {
                    gameObject.SetActive(false);
                }
            }
#elif UNITY_ANDROID
            if (Networking.LocalPlayer.IsUserInVR()) {
                if(!showInAndroidVR)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                if(!showInAndroidMobile)
                {
                    gameObject.SetActive(false);
                }
            }
#elif UNITY_IOS || UNITY_VISIONOS
            if (Networking.LocalPlayer.IsUserInVR()) {
                if(!showInVisionOS)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                if(!showInIOS)
                {
                    gameObject.SetActive(false);
                }
            }
#else
            gameObject.SetActive(false);
#endif
        }

        public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
        {
            if (player.isLocal)
            {
                if (PlayerData.HasKey(player, hintKey) && PlayerData.GetBool(player, hintKey)) {
                    gameObject.SetActive(false);
                }
            }
        }

        public override void Interact()
        {
            gameObject.SetActive(false);
            PlayerData.SetBool(hintKey, true);
        }
    }
}
