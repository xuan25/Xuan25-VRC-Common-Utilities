using UdonSharp;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PlayerVoiceSystem.Debugging
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RoomDebuggingPanel : UdonSharpBehaviour
    {
        public PlayerVoiceRoomController playerVoiceRoomController;

        public GameObject rowPrefab;

        public GameObject rowContainer;

        private RoomDebuggingRow[] rows;

        private int roomValidityMask;

        int playerCountMax;

        void Start()
        {
            if (playerVoiceRoomController == null)
            {
                Debug.LogError($"[{GetUdonTypeName()}] PlayerVoiceRoomController is not set up correctly.");
                return;
            }

            int roomCountMax = playerVoiceRoomController.playerVoiceRooms.Length;

            roomValidityMask = 0;
            for (int i = 0; i < roomCountMax; i++)
            {
                if (playerVoiceRoomController.playerVoiceRooms[i] != null)
                {
                    roomValidityMask |= 1 << i;
                }
            }

            playerCountMax = playerVoiceRoomController.playerVoiceRoomMask.Length;
            rows = new RoomDebuggingRow[playerCountMax];
            for (int i = 0; i < playerCountMax; i++)
            {
                GameObject rowObject = Instantiate(rowPrefab, rowContainer.transform);
                rowObject.SetActive(false); // Initially hide the row
                RoomDebuggingRow row = rowObject.GetComponent<RoomDebuggingRow>();
                row.Setup(roomCountMax);
                rows[i] = row;
            }

            OnPlayerListChanged();
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

            UpdatePlayerState();
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

                rows[i].SetMask(playerVoiceRoomController.playerVoiceRoomMask[i], roomValidityMask);
                // Debug.Log($"[{GetUdonTypeName()}] Updated mask for player {player.displayName} (ID: {i}) with mask: {playerVoiceRoomController.playerVoiceRoomMask[i]} and room validity mask: {roomValidityMask}");
            }
        }

    }

}