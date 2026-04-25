
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AnimatorUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DiscreteAnimatorDriverIndexerTrigger : DiscreteAnimatorDriverIndexer
    {
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            SetParameter();
        }
    }
}
