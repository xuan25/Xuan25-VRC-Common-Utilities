
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Xuan25.AnimatorUtilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DiscreteAnimatorDriverIndexerInteractable : DiscreteAnimatorDriverIndexer
    {
        public override void Interact()
        {
            SetParameter();
        }
    }
}