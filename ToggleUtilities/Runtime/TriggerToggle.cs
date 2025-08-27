
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtilities
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TriggerToggle : UdonSharpBehaviour
    {
        public bool activeStateEnter = true;

        public bool invertOnExit = true;

        public GameObject[] targets;

        void Start()
        {

        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].SetActive(activeStateEnter);
                }
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!invertOnExit)
            {
                return;
            }
            if (player.isLocal)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].SetActive(!activeStateEnter);
                }
            }
        }
    }

}