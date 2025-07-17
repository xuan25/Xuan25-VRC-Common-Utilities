
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace RandomHintDisplay
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class HintDisplay : UdonSharpBehaviour
    {
        public abstract void ShowHint(HintFeeder hintFeeder, bool sync);
    }
}
