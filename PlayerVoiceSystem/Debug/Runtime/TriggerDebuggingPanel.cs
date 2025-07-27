
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

        void Start()
        {
            if (playerVoiceTrigger == null)
            {
                Debug.LogError($"[{GetUdonTypeName()}] PlayerVoiceTrigger is not set up correctly.");
                return;
            }

            playerCountMax = playerVoiceTrigger.playerMask.Length;
            rows = new RoomDebuggingRow[playerCountMax];
            for (int i = 0; i < playerCountMax; i++)
            {
                GameObject rowObject = Instantiate(rowPrefab, rowContainer.transform);
                rowObject.SetActive(false); // Initially hide the row
                RoomDebuggingRow row = rowObject.GetComponent<RoomDebuggingRow>();
                row.Setup(1);
                rows[i] = row;
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
                    rows[i].gameObject.SetActive(false);
                    continue;
                }

                rows[i].gameObject.SetActive(true);
                rows[i].SetUserName(player.displayName);
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

                rows[i].SetMask(playerVoiceTrigger.playerMask[i] ? 1 : 0, 1);
                // Debug.Log($"[{GetUdonTypeName()}] Updated mask for player {player.displayName} (ID: {i}) with mask: {playerVoiceTrigger.playerMask[i]}");
            }
        }
    }
}
