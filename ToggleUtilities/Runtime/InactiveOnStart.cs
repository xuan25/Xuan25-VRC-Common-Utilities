
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ToggleUtilities
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InactiveOnStart : UdonSharpBehaviour
    {
        void Start()
        {
            gameObject.SetActive(false);
        }
    }

}