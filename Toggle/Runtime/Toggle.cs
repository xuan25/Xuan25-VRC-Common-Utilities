
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtility
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Toggle : ToggleBase
    {
        [SerializeField] private GameObject[] targets;

        [UdonSynced] private bool[] activeMaskGlobal = new bool[0];

        void Start()
        {
            activeMaskGlobal = new bool[targets.Length];
        }

        public override void OnToggle(bool global)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].SetActive(!targets[i].activeSelf);
            }

            if (global)
            {
                if (!Networking.IsOwner(gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                }
                for (int i = 0; i < targets.Length; i++)
                {
                    activeMaskGlobal[i] = targets[i].activeSelf;
                }
                RequestSerialization();
            }
        }

        public override void OnDeserialization()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].SetActive(activeMaskGlobal[i]);
            }
        }
    }
}