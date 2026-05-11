
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PingSender
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PingSenderCore : UdonSharpBehaviour
    {
        public const int NUM_PLAYER_MAX = 80;

        public bool[] playerIDMask = new bool[NUM_PLAYER_MAX];
                
        [SerializeField]
        public AudioSource pingAudioSource;

        [SerializeField]
        public bool verbose = false;

        private PingSenderPanel[] pingSenderPanels;

        [UdonSynced]
        private int pingingPlayerId = -1;

#region Unity lifecycle

        void Start()
        {           
            RefreshPlayers();
            UpdateUI();
        }

#endregion

#region Ping logic

        public void PingPlayer(VRCPlayerApi player)
        {
            if (player == null) return;

            if (verbose)
            {
                Debug.Log($"Pinging player {player.displayName} ({player.playerId})");
            }

            pingingPlayerId = player.playerId;

            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            RequestSerialization();
            OnDeserialization();
        }
        
        public override void OnDeserialization()
        {
            PlayPingIfTarget(pingingPlayerId);
        }

        private void PlayPingIfTarget(int playerId)
        {
            if (playerId < 0) return;
            if (Networking.LocalPlayer.playerId != playerId) return;

            if (verbose)
            {
                Debug.Log($"I am the target of the ping (playerId: {playerId}), playing ping sound");
            }

            if (pingAudioSource != null)
            {
                pingAudioSource.Play();
            }
        }

#endregion

#region Player join/leave handling

        public void RefreshPlayers()
        {
            for (int i = 0; i < NUM_PLAYER_MAX; i++)
            {
                playerIDMask[i] = false;
            }

            VRCPlayerApi[] players = new VRCPlayerApi[NUM_PLAYER_MAX];
            VRCPlayerApi.GetPlayers(players);

            for (int i = 0; i < players.Length; i++)
            {
                VRCPlayerApi player = players[i];
                if (player == null) continue;
                int playerId = player.playerId;
                if (playerId < 0 || playerId >= NUM_PLAYER_MAX) continue;

                playerIDMask[playerId] = true;
            }
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            RefreshPlayers();
            
            playerIDMask[player.playerId] = true;

            UpdateUI();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RefreshPlayers();

            playerIDMask[player.playerId] = false;

            UpdateUI();
        }

#endregion

#region Admin Panel UI

        public bool[] GetPlayerIDMask()
        {
            return playerIDMask;
        }

        public void UpdateUI()
        {
            // Notify admin panels to update
            if (pingSenderPanels == null) return;
            foreach (PingSenderPanel panel in pingSenderPanels)
            {
                panel.UpdateUI();
            }
        }

        public void RegisterPingSenderPanel(PingSenderPanel panel)
        {
            if (pingSenderPanels == null)
            {
                pingSenderPanels = new PingSenderPanel[] { panel };
            }
            else
            {
                PingSenderPanel[] panelList = new PingSenderPanel[pingSenderPanels.Length + 1];
                System.Array.Copy(pingSenderPanels, panelList, pingSenderPanels.Length);
                panelList[pingSenderPanels.Length] = panel;
                pingSenderPanels = panelList;
            }

            panel.UpdateUI();
        }

#endregion
    }

}