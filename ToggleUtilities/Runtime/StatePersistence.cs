
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class StatePersistence : UdonSharpBehaviour
    {
        [SerializeField]
        private string persistenceKey = "StatePersistenceKey";
        
        [SerializeField]
        private GameObject[] targets;

        [SerializeField]
        private GameObject[] targetsInverse;
        
        private void ApplyBindState(bool state)
        {
            if (state)
            {
                foreach (GameObject obj in targets)
                {
                    obj.SetActive(true);
                }
                foreach (GameObject obj in targetsInverse)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                foreach (GameObject obj in targets)
                {
                    obj.SetActive(false);
                }
                foreach (GameObject obj in targetsInverse)
                {
                    obj.SetActive(true);
                }
            }
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer != player) return;
            if (!PlayerData.TryGetBool(player, persistenceKey, out bool enabled)) return;

            ApplyBindState(enabled);
        }

        public void Enable()
        {
            PlayerData.SetBool(persistenceKey, true);
            ApplyBindState(true);
        }

        public void Disable()
        {
            PlayerData.SetBool(persistenceKey, false);
            ApplyBindState(false);
        }
    }
}