
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PingSender
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PingPlayerEntry : UdonSharpBehaviour
    {

        public TMPro.TextMeshProUGUI playerNameText;

        private PingSenderCore pingSenderCore;
        private VRCPlayerApi player;

        public void Setup(PingSenderCore pingSenderCore, VRCPlayerApi player)
        {
            this.pingSenderCore = pingSenderCore;
            this.player = player;

            playerNameText.text = player.displayName;
        }

        public void OnClick()
        {
            pingSenderCore.PingPlayer(player);
        }
    }

}