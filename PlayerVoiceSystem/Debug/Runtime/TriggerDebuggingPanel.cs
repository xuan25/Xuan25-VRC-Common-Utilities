
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem.Debugging
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TriggerDebuggingPanel : UdonSharpBehaviour
    {
        public PlayerVoiceTrigger playerVoiceTrigger;

        public GameObject rowPrefab;

        public GameObject rowContainer;

        private RoomDebuggingRow[] rows;

        int playerCountMax;

        void OnEnable()
        {
            if (playerVoiceTrigger == null)
            {
                Debug.LogError($"[{GetUdonTypeName()}] PlayerVoiceTrigger is not set up correctly.");
                return;
            }

            playerCountMax = playerVoiceTrigger.playerMask.Length;
            rows = new RoomDebuggingRow[playerCountMax];
        }

        private void InitializeRow(int index, int roomCount, bool isActive, string username)
        {
            if (rows[index] == null)
            {
                GameObject rowObject = Instantiate(rowPrefab, rowContainer.transform);
                RoomDebuggingRow row = rowObject.GetComponent<RoomDebuggingRow>();
                row.Setup(roomCount);
                rows[index] = row;
            }

            rows[index].gameObject.SetActive(isActive);
            if (isActive)
            {
                rows[index].SetUserName(username);
            }
        }

        private void UninitializeRow(int index)
        {
            if (rows[index] != null)
            {
                rows[index].gameObject.SetActive(false);
            }
        }

        public void OnPlayerListChanged()
        {
            // Debug.Log($"[{GetUdonTypeName()}] OnPlayerListChanged called. Updating player rows...");
            SendCustomEventDelayedFrames(nameof(UpdatePlayerList), 1);
        }

        public void UpdatePlayerList()
        {
            for (int i = 0; i < playerCountMax; i++)
            {
                VRCPlayerApi player = VRCPlayerApi.GetPlayerById(i);
                if (player == null)
                {
                    UninitializeRow(i);
                    continue;
                }

                InitializeRow(i, 1, true, player.displayName);
            }

            OnPlayerStateChanged();
        }

        public void OnPlayerStateChanged()
        {
            // Debug.Log($"[{GetUdonTypeName()}] OnPlayerStateChanged called. Updating player masks...");
            UpdatePlayerState();
        }

        public void UpdatePlayerState()
        {
            for (int i = 0; i < playerCountMax; i++)
            {
                VRCPlayerApi player = VRCPlayerApi.GetPlayerById(i);
                if (player == null) continue;

                if (rows[i] == null)
                {
                    InitializeRow(i, 1, true, player.displayName);
                }
                rows[i].SetMask(playerVoiceTrigger.playerMask[i] ? 1 : 0, 1);
                // Debug.Log($"[{GetUdonTypeName()}] Updated mask for player {player.displayName} (ID: {i}) with mask: {playerVoiceTrigger.playerMask[i]}");
            }
        }
    }
}
