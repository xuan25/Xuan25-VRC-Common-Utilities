
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AvatarAdmin
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarAdminPanelAvatarOption : UdonSharpBehaviour
    {
        [SerializeField]
        GameObject selectionHighlight;

        [SerializeField]
        TMPro.TextMeshProUGUI avatarDescriptionText;

        private AvatarAdminCore avatarAdminCore;
        private int playerId;
        private int avatarIndex;

        public void Initialize(AvatarAdminCore avatarAdminCore, int playerId, int avatarIndex)
        {
            this.avatarAdminCore = avatarAdminCore;
            this.playerId = playerId;
            this.avatarIndex = avatarIndex;

            avatarDescriptionText.text = avatarAdminCore.GetAvatarOption(avatarIndex).GetDescription();
        }

        public void UpdateUI()
        {
            uint playerAvatarIdx = avatarAdminCore.GetPlayerAvatarIndex(playerId);
            selectionHighlight.SetActive(playerAvatarIdx == avatarIndex);
        }

        public void OnSelect()
        {
            uint currentAvatarIdx = avatarAdminCore.GetPlayerAvatarIndex(playerId);
            if (currentAvatarIdx == avatarIndex)
            {
                // Deselect if already selected
                avatarAdminCore.ResetPlayerAvatar(playerId, true);
            }
            else
            {
                // Select this avatar
                avatarAdminCore.SetPlayerAvatarIndex(playerId, (uint)avatarIndex, true);
            }
        }
    }
}
