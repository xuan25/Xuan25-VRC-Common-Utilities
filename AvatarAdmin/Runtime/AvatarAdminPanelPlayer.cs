
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AvatarAdmin
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarAdminPanelPlayer : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject playerOptionEntryPrefab;

        [SerializeField]
        private Transform playerOptionEntryContainer;

        [SerializeField]
        private TextMeshProUGUI playerNameText;

        private int playerId;

        private AvatarAdminPanelAvatarOption[] avatarOptions;

        public void Initialize(AvatarAdminCore avatarAdminCore, int playerId)
        {
            this.playerId = playerId;

            for (int i = 0; i < playerOptionEntryContainer.childCount; i++)
            {
                Destroy(playerOptionEntryContainer.GetChild(i).gameObject);
            }

            int numAvatars = avatarAdminCore.GetNumAvatarOptions();
            avatarOptions = new AvatarAdminPanelAvatarOption[numAvatars];
            for (int i = 0; i < numAvatars; i++)
            {
                GameObject entry = Instantiate(playerOptionEntryPrefab, playerOptionEntryContainer);
                AvatarAdminPanelAvatarOption avatarOption = entry.GetComponent<AvatarAdminPanelAvatarOption>();

                avatarOption.Initialize(avatarAdminCore, playerId, i);
                avatarOptions[i] = avatarOption;
            }
        }

        public void UpdateUI()
        {
            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(playerId);
            if (!Utilities.IsValid(player))
            {
                playerNameText.text = "<Player not found>";
                return;
            }
            playerNameText.text = player.displayName;
            for (int i = 0; i < avatarOptions.Length; i++)
            {
                avatarOptions[i].UpdateUI();
            }

        }
    }
}