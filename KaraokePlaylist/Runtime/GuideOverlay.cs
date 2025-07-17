
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GuideOverlay : UdonSharpBehaviour
    {
        public bool overlayChecked = false;

        void Start()
        {

        }

        public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
        {
            if (player.isLocal)
            {
                if (PlayerData.HasKey(player, nameof(overlayChecked)) && PlayerData.GetBool(player, nameof(overlayChecked))) {
                    gameObject.SetActive(false);
                }
            }
        }

        public override void Interact()
        {
            gameObject.SetActive(false);
            PlayerData.SetBool(nameof(overlayChecked), true);
        }
    }

}