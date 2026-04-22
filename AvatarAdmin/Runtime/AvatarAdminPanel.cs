
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AvatarAdmin
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarAdminPanel : UdonSharpBehaviour
    {
        [SerializeField]
        private AvatarAdminCore avatarAdminCore;

        [SerializeField]
        private GameObject playerEntryPrefab;
        
        [SerializeField]
        private Transform playerEntryContainer;

        private AvatarAdminPanelPlayer[] playerEntries;

        void Start()
        {
            Initialize(avatarAdminCore);
        }

        public void Initialize(AvatarAdminCore avatarAdminCore)
        {
            this.avatarAdminCore = avatarAdminCore;
            
            for (int i = 0; i < playerEntryContainer.childCount; i++)
            {
                Destroy(playerEntryContainer.GetChild(i).gameObject);
            }

            playerEntries = new AvatarAdminPanelPlayer[AvatarAdminCore.NUM_PLAYER_MAX];
            for (int i = 0; i < AvatarAdminCore.NUM_PLAYER_MAX; i++)
            {
                GameObject playerEntryObj = Instantiate(playerEntryPrefab, playerEntryContainer);
                AvatarAdminPanelPlayer playerEntry = playerEntryObj.GetComponent<AvatarAdminPanelPlayer>();
                playerEntry.Initialize(avatarAdminCore, i);
                playerEntries[i] = playerEntry;
            }

            avatarAdminCore.RegisterAdminPanel(this);
        }

        public void UpdateUI()
        {
            for (int i = 0; i < AvatarAdminCore.NUM_PLAYER_MAX; i++)
            {
                uint avatarIdx = avatarAdminCore.GetPlayerAvatarIndex(i);
                AvatarAdminPanelPlayer playerEntry = playerEntries[i];
                if (avatarIdx == AvatarAdminCore.IDX_UNINITIALIZED)
                {
                    playerEntry.gameObject.SetActive(false);
                }
                else
                {
                    playerEntry.gameObject.SetActive(true);
                    playerEntry.UpdateUI();
                }
            }
        }
    }
}