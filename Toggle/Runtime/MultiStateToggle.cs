
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtility
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

    public class MultiStateToggle : ToggleBase
    {
        [SerializeField] private GameObject[] targets;

        [UdonSynced] private int activeIndexGlobal = 0;

        private int activeIndexLocal = 0;

        void Start()
        {
            ApplyActiveIndex(activeIndexLocal);
        }

        public override void OnToggle(bool global)
        {
            activeIndexLocal++;
            if (activeIndexLocal >= targets.Length)
            {
                activeIndexLocal = 0;
            }

            if (global)
            {
                if (!Networking.IsOwner(gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                }
                activeIndexGlobal = activeIndexLocal;
                RequestSerialization();
            }

            ApplyActiveIndex(activeIndexLocal);
        }

        public override void OnDeserialization()
        {
            activeIndexLocal = activeIndexGlobal;
            ApplyActiveIndex(activeIndexGlobal);
        }

        public void ApplyActiveIndex(int index)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].gameObject.SetActive(i == index);
                }
            }
        }
    }

}