using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AvatarAdmin
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AvatarAdminCore : UdonSharpBehaviour
    {
        public const int NUM_PLAYER_MAX = 1000;
        public const int IDX_UNINITIALIZED = int.MaxValue;
        public const int IDX_UNSET = int.MaxValue - 1;


        // List of avatar options
        [SerializeField]
        private AvatarAdminAvatarOption[] avatarOptions;

        [SerializeField]
        public bool verbose = false;

        // playerId to avatarIndex mapping, 0 means no change, index starts from 1
        [UdonSynced]
        [HideInInspector]
        public uint[] playerAvatarIdx = new uint[NUM_PLAYER_MAX];

        private AvatarAdminPanel[] adminPanels;

        private uint currentLocalAvatarIdx = IDX_UNSET;

#region Unity lifecycle

        void Start()
        {
            // Initialize playerAvatarIdx to uninitialized state
            for (int i = 0; i < NUM_PLAYER_MAX; i++)
            {
                playerAvatarIdx[i] = IDX_UNINITIALIZED;
            }

            RefreshPlayerList(Networking.IsOwner(gameObject));
            UpdateUI();
        }

#endregion

#region Avatar options

        public int GetNumAvatarOptions()
        {
            return avatarOptions.Length;
        }

        public AvatarAdminAvatarOption GetAvatarOption(int index)
        {
            if (index < 0 || index >= avatarOptions.Length) return null;
            return avatarOptions[index];
        }

#endregion

#region Global avatar admin

        public void SetPlayerAvatarIndex(int playerId, uint avatarIndex, bool sync = false)
        {

            if (playerId < 0 || playerId >= NUM_PLAYER_MAX) return;

            if (sync && !Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            if (verbose) Debug.Log($"{nameof(AvatarAdminCore)}: Setting avatar index {avatarIndex} for player {playerId}");
            playerAvatarIdx[playerId] = avatarIndex;

            if (sync)
            {
                RequestSerialization();
                OnDeserialization();
            }
        }

        public void ResetPlayerAvatar(int playerId, bool sync = false)
        {
            SetPlayerAvatarIndex(playerId, IDX_UNSET, sync);
        }

        public void ReleasePlayerAvatar(int playerId, bool sync = false)
        {
            SetPlayerAvatarIndex(playerId, IDX_UNINITIALIZED, sync);
        }

        public uint GetPlayerAvatarIndex(int playerId)
        {
            if (playerId < 0 || playerId >= NUM_PLAYER_MAX) return IDX_UNINITIALIZED;
            return playerAvatarIdx[playerId];
        }

#endregion

#region Local avatar admin

        private void RefreshLocalAvatar(bool forceReApply = false)
        {
            // update change local avatar if the flag corresponding to local player is set
            int localPlayerId = Networking.LocalPlayer.playerId;
            uint avatarIndex = GetPlayerAvatarIndex(localPlayerId);
            if (avatarIndex == IDX_UNSET || avatarIndex == IDX_UNINITIALIZED) return; // No change
            if (!forceReApply && currentLocalAvatarIdx == avatarIndex) return; // Already using the correct avatar

            AvatarAdminAvatarOption option = avatarOptions[avatarIndex];
            option.SetAvatarUse();
            currentLocalAvatarIdx = avatarIndex;
        }

        public override void OnDeserialization()
        {
            UpdateUI();
            RefreshLocalAvatar(false);
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer.playerId != player.playerId) return;
            RefreshLocalAvatar(true);
        }

#endregion

#region Player state initialization and cleanup

        public void RefreshPlayerList(bool sync = false)
        {
            for (int i = 0; i < NUM_PLAYER_MAX; i++)
            {
                VRCPlayerApi player = VRCPlayerApi.GetPlayerById(i);
                if (!Utilities.IsValid(player))
                {
                    // Player slot is empty, release to uninitialized
                    ReleasePlayerAvatar(i, false);
                    continue;
                }
                // If player is valid but avatar index is uninitialized, set to unset
                if (playerAvatarIdx[i] == IDX_UNINITIALIZED) {
                    ResetPlayerAvatar(i, false);
                }
                // otherwise keep the current avatar index (including unset) to avoid unnecessary avatar reload
            }

            if (sync)
            {
                RequestSerialization();
                OnDeserialization();
            }
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            // Reset flag on resource init
            if (!Networking.LocalPlayer.IsOwner(gameObject)) return;
            RefreshPlayerList(true);
            ResetPlayerAvatar(player.playerId, true);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            // Reset flag on resource release
            if (!Networking.LocalPlayer.IsOwner(gameObject)) return;
            RefreshPlayerList(true);
            ReleasePlayerAvatar(player.playerId, true);
        }

#endregion

#region Admin Panel UI

        public void UpdateUI()
        {
            // Notify admin panels to update
            if (adminPanels == null) return;
            foreach (AvatarAdminPanel panel in adminPanels)
            {
                panel.UpdateUI();
            }
        }

        public void RegisterAdminPanel(AvatarAdminPanel panel)
        {
            if (adminPanels == null)
            {
                adminPanels = new AvatarAdminPanel[] { panel };
            }
            else
            {
                AvatarAdminPanel[] panelList = new AvatarAdminPanel[adminPanels.Length + 1];
                System.Array.Copy(adminPanels, panelList, adminPanels.Length);
                panelList[adminPanels.Length] = panel;
                adminPanels = panelList;
            }

            panel.UpdateUI();
        }

#endregion

    }
}