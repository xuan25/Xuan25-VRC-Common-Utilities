
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.PingSender
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PingSenderPanel : UdonSharpBehaviour
    {
        [SerializeField]
        private PingSenderCore pingSenderCore;

        [SerializeField]
        private GameObject playerEntryPrefab;
        
        [SerializeField]
        private Transform playerEntryContainer;

        void Start()
        {
            Initialize(pingSenderCore);
        }

        public void Initialize(PingSenderCore pingSenderCore)
        {
            this.pingSenderCore = pingSenderCore;

            pingSenderCore.RegisterPingSenderPanel(this);
        }

        public void UpdateUI()
        {
            for (int i = 0; i < playerEntryContainer.childCount; i++)
            {
                Destroy(playerEntryContainer.GetChild(i).gameObject);
            }

            bool[] playerIDMask = pingSenderCore.GetPlayerIDMask();
            
            for (int i = 0; i < playerIDMask.Length; i++)
            {
                if (!playerIDMask[i]) continue;

                VRCPlayerApi player = VRCPlayerApi.GetPlayerById(i);
                if (player != null)
                {
                    GameObject playerEntryObj = Instantiate(playerEntryPrefab, playerEntryContainer);
                    PingPlayerEntry playerEntry = playerEntryObj.GetComponent<PingPlayerEntry>();
                    playerEntry.Setup(pingSenderCore, player);
                }
            }
        }
    }

}